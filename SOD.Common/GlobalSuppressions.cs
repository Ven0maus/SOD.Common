// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Most of these statics are suppressed because they should be instances to make the methods easy to access from one place
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Shadows.Implementations.GameMessage.Broadcast(System.String,InterfaceController.GameMessageType,InterfaceControls.Icon,System.Nullable{UnityEngine.Color},System.Single)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~P:SOD.Common.Shadows.Implementations.Time.IsInitialized")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Shadows.Implementations.SaveGame.GetSavestoreDirectoryPath(System.Reflection.Assembly)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:SOD.Common.Shadows.Implementations.SaveGame.GetUniqueString(System.String)~System.String")]
