using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Common.Helpers.ObjectiveObjects
{
    public sealed class ObjectiveBuilder
    {
        private readonly Case _case;
        private string _text;
        private Vector3? _pointerPosition;
        private float _delay = 0;
        private bool _removePrevious;
        private InterfaceControls.Icon _icon;
        private Objective.ObjectiveTrigger _trigger;
        private Action<Objective> _onComplete, _onCancel;
        private List<(ObjectiveBuilder, ChildTrigger)> _children;

        /// <summary>
        /// The parent objective, which will excute this objective.
        /// </summary>
        internal ObjectiveBuilder Parent { get; private set; }

        internal ObjectiveBuilder(Case @case)
        {
            if (@case == null)
                throw new ArgumentException("Case cannot be null.", nameof(@case));
            _case = @case;
        }

        /// <summary>
        /// Set's the text of the objective.
        /// </summary>
        /// <param name="text"></param>
        public ObjectiveBuilder SetText(string text)
        {
            _text = text;
            return this;
        }

        /// <summary>
        /// Set the icon of the objective.
        /// </summary>
        /// <param name="icon"></param>
        public ObjectiveBuilder SetIcon(InterfaceControls.Icon icon)
        {
            _icon = icon;
            return this;
        }

        /// <summary>
        /// Set this to make this objective remove all the previous objectives when it is created.
        /// </summary>
        public ObjectiveBuilder RemovePreviousObjectives()
        {
            _removePrevious = true;
            return this;
        }

        /// <summary>
        /// Make's the objective only appear after the given delay in seconds is passed.
        /// <br>If this method is not set, it will be shown instantly.</br>
        /// </summary>
        /// <param name="delay"></param>
        public ObjectiveBuilder SetDelay(float delay)
        {
            _delay = delay;
            return this;
        }

        /// <summary>
        /// Allow a fully customizable trigger to be set.
        /// </summary>
        /// <param name="trigger"></param>
        public ObjectiveBuilder SetCompletionTrigger(Objective.ObjectiveTrigger trigger)
        {
            _trigger = trigger;
            return this;
        }

        /// <summary>
        /// Set a predefined trigger provided by SOD.Common for ease of use, not all types are available.
        /// <br><see cref="PredefinedTrigger"/> has a set of static methods that provide triggers, example: <see cref="PredefinedTrigger.PickupInteractable(Interactable, bool)"/>.</br>
        /// <br>For more customizability, create an <see cref="Objective.ObjectiveTrigger"/> manually and use the overload method.</br>
        /// </summary>
        /// <param name="predefinedTrigger">A set of predefined triggers provided by SOD.Common</param>
        public ObjectiveBuilder SetCompletionTrigger(PredefinedTrigger predefinedTrigger)
        {
            _trigger = predefinedTrigger.ObjectiveTrigger;
            return this;
        }

        /// <summary>
        /// Set an action to be executed on completion of this objective.
        /// </summary>
        /// <param name="action"></param>
        public ObjectiveBuilder SetCompleteAction(Action<Objective> action)
        {
            _onComplete = action;
            return this;
        }

        /// <summary>
        /// Set an action to be executed on cancelation of this objective.
        /// </summary>
        /// <param name="action"></param>
        public ObjectiveBuilder SetCancelAction(Action<Objective> action)
        {
            _onCancel = action;
            return this;
        }

        /// <summary>
        /// Can be set to show a pointer to the given position.
        /// <br>Note: some <see cref="PredefinedTrigger"/>'s do this automatically.</br>
        /// </summary>
        /// <param name="position"></param>
        public ObjectiveBuilder SetPointer(Vector3 position)
        {
            _pointerPosition = position;
            return this;
        }

        /// <summary>
        /// Add's a child objective that will be triggered based on the selected <see cref="ChildTrigger"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="trigger">When the objective should be created.</param>
        public ObjectiveBuilder AddChild(ObjectiveBuilder builder, ChildTrigger trigger)
        {
            if (builder._registered)
                throw new Exception("This builder has already been registered, and cannot be added as a child.");
            if (builder.Parent != null)
                throw new Exception("This builder already has a parent objective, and cannot be added as a child.");

            // Set parent
            builder.Parent = this;

            // Add as a child
            _children ??= new List<(ObjectiveBuilder, ChildTrigger)>();
            _children.Add((builder, trigger));

            return this;
        }

        private bool _registered = false;

        public void Register()
        {
            if (Parent != null)
                throw new Exception("This builder cannot be registered as it is a child of another builder, and will be registered automatically.");

            RegisterInternal();
        }

        /// <summary>
        /// Register's the new objective to be created in the game.
        /// </summary>
        internal void RegisterInternal()
        {
            if (_registered)
                throw new Exception("This builder was already registered, and cannot be registered again.");

            // Generate a unique dds identifier
            var entryRef = Guid.NewGuid().ToString();
            while (Lib.CaseObjectives.CustomObjectives.ContainsKey(entryRef))
                entryRef = Guid.NewGuid().ToString();

            // Keep custom objective in a memory to trigger actions later
            var customObjective = new CustomObjective(entryRef, _case, _onComplete, _onCancel, _children);
            Lib.CaseObjectives.CustomObjectives.Add(entryRef, customObjective);

            // Create DDS strings record
            Lib.DdsStrings[customObjective.DictionaryRef, entryRef] = _text;

            // Add objective to the case
            _case.AddObjective(entryRef, _trigger, _pointerPosition != null, _pointerPosition ?? default, _icon, Objective.OnCompleteAction.nothing, _delay, _removePrevious, "", false, false, null, false, false, true);
            _registered = true;
        }

        public enum ChildTrigger
        {
            OnCompletion,
            OnCancelation
        }
    }
}
