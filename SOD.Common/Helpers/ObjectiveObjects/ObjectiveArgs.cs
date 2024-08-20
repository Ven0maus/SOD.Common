using System;

namespace SOD.Common.Helpers.ObjectiveObjects
{
    public sealed class ObjectiveArgs : EventArgs
    {
        /// <summary>
        /// The case the objective is assigned to.
        /// </summary>
        public Case Case => Objective.thisCase;

        /// <summary>
        /// The objective.
        /// </summary>
        public Objective Objective { get; }

        /// <summary>
        /// The unique identifier, used to lookup the text value in the dds records.
        /// <br>For custom objectives this is a unique generated guid.</br>
        /// </summary>
        public string EntryRef => Objective.queueElement.entryRef;

        /// <summary>
        /// The strings dictionary that is searched in, can be null incase the objective is silent.
        /// </summary>
        public string DictionaryRef
        {
            get
            {
                if (!Objective.queueElement.isSilent)
                {
                    if (Case.caseType == Case.CaseType.mainStory)
                        return ChapterController.Instance.loadedChapter.dictionary;
                    if (Objective.queueElement.useParsing)
                        return CustomObjective.MainDictionary;
                }
                return null;
            }
        }

        /// <summary>
        /// The displayed text in-game.
        /// </summary>
        public string Text => DictionaryRef != null ? Lib.DdsStrings[DictionaryRef, EntryRef] : EntryRef;

        /// <summary>
        /// Is the raised objective a custom created one?
        /// </summary>
        public bool IsCustom { get; }

        internal ObjectiveArgs(Objective objective, bool isCustom) 
        {
            Objective = objective;
            IsCustom = isCustom;
        }
    }
}
