/*
 * This file is used to import all the necessary namespaces and classes that are used in the plugin.
 * This file is then imported in ALL the files in the plugin.
 *
 * you never have to worry about importing the same namespaces in every file. Especially usefull für utility classes.
 */

global using Dalamud.Plugin;
global using Dalamud.Interface.Windowing;
global using Dalamud.Interface;
global using Dalamud.Bindings.ImGui;
global using Dalamud.Plugin.Services;
global using Dalamud.Game.ClientState.Conditions;

global using System;
global using System.Linq;
global using System.Numerics;

// Ecommons
// global using ECommons;
// global using ECommons.DalamudServices;
// global using ECommons.ImGuiMethods;
// global using static ECommons.GenericHelpers;
