// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Most of these statics are suppressed because they should be instances to make the methods easy to access from one place
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.GameMessage.Broadcast(System.String,InterfaceController.GameMessageType,InterfaceControls.Icon,System.Nullable{UnityEngine.Color},System.Single)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~P:SOD.Common.Helpers.Time.IsInitialized")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.SaveGame.GetSavestoreDirectoryPath(System.Reflection.Assembly,System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.SaveGame.GetUniqueString(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.Time.ResumeGame")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.Time.PauseGame(System.Boolean,System.Boolean,System.Boolean)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Helpers.SyncDisks.Builder(System.String)~SOD.Common.Helpers.SyncDiskObjects.SyncDiskBuilder")]
