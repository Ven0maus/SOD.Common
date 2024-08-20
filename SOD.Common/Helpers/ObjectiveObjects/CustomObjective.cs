using System;
using System.Collections.Generic;

namespace SOD.Common.Helpers.ObjectiveObjects
{
    internal sealed class CustomObjective
    {
        internal const string MainDictionary = "missions.postings";

        internal string EntryRef { get; }
        internal string DictionaryRef 
        { 
            get
            {
                if (Case.caseType == Case.CaseType.mainStory)
                    return ChapterController.Instance.loadedChapter.dictionary;
                return MainDictionary;
            } 
        }

        internal Case Case { get; }
        internal Action<Objective> OnComplete { get; }
        internal Action<Objective> OnCancel { get; }
        internal List<(ObjectiveBuilder Builder, ObjectiveBuilder.ChildTrigger Trigger)> Children { get; }

        internal CustomObjective(string entryRef, Case @case, Action<Objective> onComplete, Action<Objective> onCancel, List<(ObjectiveBuilder, ObjectiveBuilder.ChildTrigger)> children)
        {
            EntryRef = entryRef;
            Case = @case;
            OnComplete = onComplete;
            OnCancel = onCancel;
            Children = children;
        }
    }
}
