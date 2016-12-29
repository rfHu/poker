GameFlow v0.9.9b
-----------------

GameFlow is a a powerful yet easy to use visual programming toolkit designed to help you bring your game prototypes to life in the shortest possible time.


Installation 
--------------

IMPORTANT: GameFlow requires Unity 5.4 or newer. Please upgrade Unity before importing or updating the GameFlow package.

Once purchased on the Unity Asset Store and downloaded the GameFlow package should be already installed on your system and ready to use in your projects. The only required step is to import the package into your project in one of these two ways:

1) If you are creating a new project, just mark the checkbox "GameFlow.unityPackage" in the New Project window to get the package automatically imported.

2) If you need to import GameFlow into an existing project, log in the Asset Store (Window > Asset Store) and toggle the Download Manager, then Find the GameFlow package and click on the Import button to get the package automatically imported into the current project.

If you need further information on installation or the update process, please refer to the "First Steps" chapter in the User Manual under the GameFlow/Documentation folder in the Project tab.


Help & Documentation
---------------------

Reference help is always available as the last option in the context menu of any GameFlow component. The context menu is displayed by clicking any on the "Gear" icon in the upper right corner of any component or block in the Inspector tab.

Under the GameFlow/Documentation folder you will find links to the latest versions of the User Manual (PDF). Reference pages (HTML) are also included for offline browsing.


Useful Links
-------------

Overview -> http://evasiongames.com/gameflow
Documentation -> http://evasiongames.com/gameflow/docs
Tutorials -> http://evasiongames.com/gameflow/tutorials
Support -> http://evasiongames.com/gameflow/support
Forum -> http://evasiongames.com/forum
Facebook Group -> https://www.facebook.com/groups/gameflow/
Facebook Group (es) -> https://www.facebook.com/groups/gameflow.es/


Follow us:
-----------

Twitter -> https://twitter.com/evasiongames
Facebook -> https://www.facebook.com/evasiongames
Youtube -> https://www.youtube.com/user/evasiongames


Release Notes
--------------

[0.9.9b]
* GameFlow is now ready for Unity 5.5.
* Unity API Upgrade dialog should no longer appear.
* Property "Web Security Enabled" in Get Application Property action is obsolete in Unity 5.5.
* Property "Use Cone Friction" in Set / Get Rigidbody Property actions is obsolete in Unity 5.5.
* Property "Playback Speed" in Set / Get Particlle System Property actions renamed to "Simulation Speed" in Unity 5.5.
* To unclutter the Inspector, event program parameters are now hidden by default. There's now a new subtle icon which toggles parameters visibility.
* Descriptions for Variables and Parameters now using a smaller font.
* Rewording and visual refinement of warning messages shown inside programs and actions.
* Rewording and compacting of the listening range / target fields in event programs.
* Minor visual adjustment (lines, spaces, ...) in programs and other block containers.
* Fixed glitch in Unity 5.5 whereby the vertically space at the bottom of the last component shown in the Inspector is shorten in 2 pixels.
* Drag and drop line indicator now using Unity style and adjusted to match block insertion cursor.
* Console no longer cleaned after importing a new version of GameFlow.
* New Set / Get Canvas Scaler Property actions.
* Interpolate Value action now supporting Easing.
* Fixed built-in Mouse Delta variables getting wrong initial values.
* Fixed error on console on entering play mode when the listening range of an event program is set to List and no target list is specified.
* Fixed incoherence in the foldout state of blocks when showing the same GameObject in multiple Inspector windows simultaneously.
* Fixed Overview stuff not respecting the Editor Language set in Localization Settings.
* Fixed minor issues in the enable toggle of actions and other activable blocks.

[0.9.8b]
* Added keyboard support for program editing in the Inspector. Click a block title to focus it, use [Up/Down] to move the cursor and [Left/Right] to collapse/expand. Use [Return] to insert a new block in the current cursor position and [Delete] to remove the focused block. Undo is supported.
* Improved Copy/Cut/Paste support. It is now integrated with the system clipboard and yes, you can use the standard keyboard shortcuts.
* Redesigned the Add Action / Condition selector windows to be more keyboard-friendly. Selected block type help summary is now displayed at the bottom like in the good old days ;)
* Added help button in Add Action / Condition selectors for easier access. Gear icons and context menus removed.
* You can now add actions and conditions also with double click in Add Action / Condition selectors.
* Fixed the Add Action / Condition selector not remembering the last text search in some cases.
* You can now select next tab in Object selector window with the Tab key.
* Added "Help Language" option in Edit > Project Settings > Localization for selecting the language of help summaries in the Editor.
* You can now open the object selector window using Space or Return keys in focused object fields and fields that have been assigned a variable.
* You can now create variables and other objects automatically pressing Return key while the field is focused. The field needs to have a [+] button on the right.
* Variables created via the [+] button integrated in Variable-type fields (for instance, Get actions output field) will have automatically asigned an identifier in the form of "VariableXXXX" where XXXX is a random-generated number.
* You can now use Ctrl / Cmd + Return / Space to open the object selector in any field which has a default function for the Return or Space key (e.g. Color fields, Toggle fields, etc.).
* You can now open the expanded multi-line editing window pressing Alt + Return while the textfield is focused.
* Added the "missing" Copy option to the context menu of all block types.
* Added Paste option to context menu in empty area of all block sections.
* Changed visual appearance of "Add ..." buttons in all block sections.
* Changed visual appearance of "Execute" button in Program component.
* Restyling of block section titles (actions, conditions, variables, etc.). No longer using a foldout but a grayed mini label.
* Drag and drop of an action from one program to another in the same GameObject will no longer duplicate the action.
* Drag and drop of blocks is no longer enabled during play mode.
* On Click event will be now triggered *also* when pressing enter key having a button selected. This is a convenience for easier creation of keyboard-navigable menus.
* New Get Child and Get First / Last Child actions.
* New Add Hierarchy To List and Add Selection To List actions.
* Added 14 new actions related to Audio components.
* Added Random Wait action.
* New Fade In / Out actions to make an object or UI element appear or disappear gradually.
* New Interpolate Value action to gradually change the value of a variable. Valid for numeric and Color types.
* Renamed condition "Animator State" and "Current State" both as "State".
* Removal of some unnecessary labels and minor visual changes in the layout of properties in conditions.
* Added right indentation to enum property fields in actions for aesthetics.
* Added [+] button on the right of the target object property for the following actions and conditions: Add Force, Add Force 2D, Set Variable Value, Get First Raycast Hit, Get Raycast Hits, Get Raycast Hits Count, Get Ray Point, Ray Condition, Key Condition, Get Object From Pool, On State, Set State As Current, State Condition.
* Added icon with context menu on the right of the Actions, Parameters and States foldout titles offering Enable and Collapse All options. Removed these options from the component context menu.
* Added options to create Overview and State Machine to GameObject > GameFlow menu.
* The Set Current State action now implies breaking the execution of the current state action sequence. In other words, the state change is immediate.
* Added Set Next State action to have an alternative non-breaking version of the new Set Current State action. In other words, the state will change once the current state have finished its action sequence.
* Added defensive code for a rare case where a variable could lose its enum or object type giving an error in console.
* Fixed some cases of drag and drop with blocks not working properly.
* Fixed On Mouse programs (Down, Enter, etc.) not working with Collider 2D components.
* Fixed block duplicates not getting the foldout state of the original block.
* Fixed rare error on console related to validation of the Clear Particles action.
* Fixed wrong positioning of types Vector4 and Quaternion in Localizations.
* Fixed persistent variables on prefabs not restoring its value properly.
* Fixed error when parsing the saved value of a persistent variable for types Vector4 and Quaternion.
* Fixed error when casting values in variables changed from type Integer to type Layer if the value was >= 32.
* Fixed warnings in console when double clicking a property field containing a reference to a Variable, Parameter or any other block type.
* Fixed error when changing the type of a non-empty List.
* Fixed errors in List Condition when using Animation Curve, Bounds, Quaternion or Vector4 type lists and also when changing the comparison.
* Fixed List Condition treating comparison field values as bitmasks instead of enums for Enum type lists.
* The Set Material action is now executable in Editor.
* Menu option GameObject > GameFlow > Macro is now available under Assets > Create > GameFlow > Macro as it creates a Prefab containing with the Macro.
* Removed menu option Assets > Instantiate Prefab to be compliant with Asset Store guidelines.
* Removed Settings foldout title in On ... programs for visual coherence.
* Removed annoying numeric info messages on Console when opening the actions / conditions selector window.
* Added basic example B19 - Game Menu.
* Added basic example B20 - Sprite Click.

[0.9.5b]
* Minimum required Unity version is now 5.4.0. API update warning should no longer appear on import.
* Fixed issue with Get Light Property action that prevented iOS builds to complete. Property "Area Size" will be no longer available.
* Fixed internal Unity errors in Console when dragging an action or condition from the Selector window over the Scene view.
* Fixed issue whereby help summaries and pages were not showing if user system language was not English nor Spanish.
* Fixed improper validation error when using the Look At action with mode = Instant in an Update type program.
* Fixed improper validation errors when using time actions (Move, Rotate, Wait, Interpolate Color, etc.) in a State. Fixed related example B14 - State Machines.
* Fixed crash when dragging an asset from project into a field in the Inspector.
* Added property "Skin Width" to Set / Get Character Controller Property actions.
* Added property "Contact Offset" to Set / Get Collider Property actions.
* Localizations now supporting AnimationCurve, Bounds, Quaternion and Vector4 types.
* Fixed Variables and Parameters showing the "Persistent" toggle for Animation Curve type which is not a persistent type.
* Lists now supporting AnimationCurve, Bounds, Quaternion and Vector4 types. Updated all list-related actions.
* Destroy action now supporting destroying components.
* New Add Component action.
* Set / Get Script Property actions now supporting AnimationCurve, Bounds, Quaternion and Vector4 types.
* Invoke Method action now accepting Components and not only MonoBehaviours.
* Invoke Method action now supporting AnimationCurve, Bounds, Quaternion and Vector4 types for parameters and return type.
* Invoke Method and Set / Get Script Property actions no longer showing internal GameFlow stuff.
* Minor changes in visual styles for output variable title and Invoke Method parameters titles.
* Fixed issue whereby an Event Program with listening target set to 'GameObject Hierarchy' would not receive events from GameObjects added at its hierarchy at runtime.
* New Set / Get Canvas Group Property actions.
* New Set / Get Joint Property actions.
* New Set / Get Joint 2D Property actions.
* Fixed memory leaks in List after performing clear or removal operations when (and only when) list type is ScriptableObject or derived. The only affected action is Get Raycast Hits when output is set to 'Raycast Hits'.
* New Find Shader and Enable / Disable Shader Keyword actions.
* New Shader and Shader Keyword condition.
* Improved validation of time-based actions (like Wait or Interpolate) in non compatible programs (On Awake, On Destroy, On Update, On Fixed Update and On Late Update). An error message will no longer appear, instead duration is automatically set to zero to indicate that the action will be instantly executed.
* Minor visual improvements to Look At and Follow Path actions.
* Freeze action is now not executable in Editor.
* Fixed RayCasting actions not working properly when the main Camera uses orthographic projection.
* Minor visuals improvements to Ray blocks in the Inspector.
* New Set / Get Texture Property actions. Valid for: Texture, Texture2D, Texture3D and RenderTexture types.
* Fixed size for icons in object fields (i.e. texture mini thumbs).
* Fixed Object Selector window showing assets in the Scene and GameObject tabs.
* New Create / Clone / Copy Material actions.
* New Scroll Material Texture action. Ideal for background scroll ;)
* New Set / Get Material Property actions. Supports both material and named shader properties.
* Fixed minor visual issues in Overview and component-window synchronization.
Eliminados warnings construcción DLL.
* New Get Ray Point action.
* Fixed Get First Raycast Hit not always returning the first hit.
* Changes in Ray: Forced Main Camera as origin when using Screen or Viewport points as direction. Removed Screen / Viewport points options for origin.
* Fixed Object Selector window showing duplicates in Assets tab when the object type is Component.
* Minor visual fixes in Object Selector window.
* Added basic example B18: Scroll Texture.
* Added intermediate example I10: Pick Object. 

[0.9.4b]
* New 2D Support with 16 new actions: Set / Get Area Effector 2D, Set / Get Constant Force 2D Property, Set / Get Platform Effector 2D Property, Set / Get Point Effector 2D Property, Set / Get Rigidbody 2D Property, Get Sprite Property, Set / Get Sprite Renderer Property, Set / Get Surface Effector 2D Property, Get Random Position 2D.
* New 2D versions of all Trigger and Collision event programs.
* Added 2D versions of Set / Get / Limit Velocity actions.
* Added 2D versions of the Add Force, Set / Get Velocity, Sleep, Sleep List, Wake Up and Wake Up List.
* New Attract and Attract 2D actions.
* Added Basic/B17 example showing the usage of the new Attract action.
* Variables now supporting new types: Bounds, Quaternion, Animation Curve and Vector4.
* New Flip action to reverse the orientation of an object.
* New Set / Get Magnitude (for vectors) actions.
* New Set / Get Program Property actions.
* New Set / Get Lens Flare Property actions.
* New Set / Get Mesh Filter Property actions.
* New Set / Get Particle System Property actions.
* New Get / Set Text Mesh Property actions.
* New Set / Get Character Controller Property actions.
* New Set / Get Light Property actions.
* New Set / Get Line Renderer Property actions.
* New Set / Get Mesh Renderer Property actions.
* New Set / Get Nav Mesh Agent Property actions.
* New Set / Get Nav Mesh Obstacle Property actions.
* New Set / Get Off Mesh Link Property actions.
* New Set / Get Outline Property actions.
* New Set / Get Projector Property actions.
* New Get Random Position action.
* New Set / Get Shadow Property actions.
* New Set / Get Skinned Mesh Renderer Property actions.
* New Set / Get Skybox Property actions.
* New Set / Get Trail Renderer Property action.
* New Set / Get Wind Zone Property actions.
* New Set / Get Collider 2D Property actions.
* New Get Application Property action.
* New Get Direction action (for vectors).
* New Set / Get Time Property actions.
* New Get Input Axis Value action.
* New Capture Screenshot action.
* Property "Bounds" is now supported in Get Collider Property action.
* Actions and conditions are no longer available in the Component > GameFlow menu and neither through the Add Command button. To add actions you now need to create a program or macro first.
* Fixed glitch in the Inspector when adding new actions / conditions to a program.
* Fixed issue that prevented built-in variables to be recognized when referencing them in variable expansion. Now strings like ${Day}/${Month}/${Year} should just work.
* The Variable types are now alphabetically sorted in the Type popup.
* Last search text is now saved separately for Actions and Conditions selector windows.
* Fixed incorrect values when setting the "Contraints" property with Set Rigidbody Property action.
* Fixed error when using Interpolate Color in a State.
* Time-based actions like Wait, Move, Rotate, etc. will be automatically disabled when used in States or other non compatible Programs like On Update. For information, a validation error message will be shown in the Inspector that can be removed by adjusting the Duration of the action to zero.
* Fixed Get Angle not returning the correct angle in some edge cases.
* Invoke Method now logging catched exceptions to console.
* Fixed Ray Condition not working.
* Fixed error in Evaluate Math Function action.
* Removed the Debug > Capture Inspectors menu option. You can use XT Utilities (free asset) for this.
* Fixed all built-in Mouse Delta variables not returning proper values.
* Minor changes in Set Distance action.
* Added additional line feed to messages sent to console for clearness.
* Wait property in Execute Program action won't show if used inside an On Update, On Fixed Update, On Late Update programs or a State.
* New Screen Center built-in variable.
* Fixed wrong angles when using relative rotation in Rotate and Rotate Randomly.
* Added optional rotation interpolation to the Look At action supporting time or speed and easing. Removed the Look Direction property.
* Fixed wrong volume when setting a value for Fade Duration in the Play Music action.

[0.9.2b]
* WebGL export now fully working.
* Optimized block drawing in the Inspector window. Editing and scrolling programs with many blocks should be noticeable faster.
* Added descriptions to all event parameters (note: you need to unfold a parameter to see its description).
* New On Variable Change event program (works only on runtime).
* New Evaluate External action. Evaluates Javascript code in the context of the web container.
* New On External Message event program allowing to receive messages sent by the web container.
* New actions and conditions allowing for easy networking: Create Web Request, Send Web Request, Get Web Request Respose / Property, Cancel Web Request, Web Request Condition.
* New Get JSON From List action.
* New Toggle Component Enablement action.
* Destroy action now supports Undo if executed in Editor mode.
* Added Is / Is Not Empty comparisons to Variable Value Condition (applies only to String type variables).
* Set Color Component now supporting integer or float values.
* Evaluate Math Function now supporting also Integer output type (just specify a Integer variable as output).
* Added output type selection (integer or float) to Get Color Component.
* Added position type selection (Vector3 or Transform) to Clone and Instantiate actions.
* Added a new "Ignore Pause" property to the Execute Program action to decide whether the Target program should ignore a Pause Game.
* On Destroy programs are now correctly executed when destroying -using the Destroy action- GameObjects belonging to a Pool.
* Fixed issue whereby Update programs and time actions were ignoring Pause Game. Now the only event program than will ignore pause by default is On Game Pause.
* Fixed issue whereby a persistent Variable which type had been changed in runtime would not restore its last saved value. To prevent this issue, type change has been disallowed for persistent Variables.
* Fixed Unity hanging after pausing the Editor if there was any On Draw Gizmos program in execution.
* Fixed "Ignore Pause" option not working in Timers.
* Due to performance problems, On Draw Gizmos programs will no longer be executed in Play mode. Remember On Draw Gizmos is experimental yet, so please use it with caution.
* Fixed warning message after clicking in color fields when using the native color selector in OSX.
* Fixed issues when moving a block with drag and drop to a different destination container or block section.
* Fixed issue that prevented correct drag and drop of GameFlow Variables into public Variable type script fields.
* Known issue: Moving a block from a Program/StateMachine to another Program/StateMachine will copy the block.
* Fixed minor glitch on the "Hide" button inside Else section of the If action.
* Fixed issue when casting a variable to an Enum type.
* Fixed minor glitch in the program parameter value field.
* Fixed minor label glitch in Set Color Component.
* Fixed localized warning text not properly displayed on the console.
* Added additional checks in context menu options to avoid conflict with other plugins.
* Removed unnecessary GameFlow entries in Gizmos menu.
* Fixed Audio initialization problems.
* Added 3D Snake example game.

[0.9.1b]
* IMPORTANT: Dropped support for Unity 4.x, minimum supported version is now 5.1.
* New Ray component to define rays that you can use with the new raycasting actions and conditions.
* New Ray Condition to check whether a casted ray intersects a specified collider or any collider, supporting layer filtering.
* New Get / Set Ray Property actions.
* New Get Raycast Hits action.
* New Get Raycast Hits Count action.
* New Get First Raycast Hit action.
* New Get Raycast Hit Property action.
* New Insert In String action.
* New Set Substring action.
* New Invert String action.
* Added new string comparisons (Contains, Starts With, Ends With) to Variable / Parameter Value conditions.
* New Evaluate Math Function action (Sine, Cosine, Absolute, Sign, Power, etc.)
* New Get Integer / Fractional Part actions.
* New Round Variable Value action.
* New Set Mouse Cursor action (for @melwalsinats with love ;).
* New Open URL action.
* New Send Command To List action.
* Break can now be used within a State to stop its execution.
* Set Variable Value now supporting Format for String type variables when the value to set comes from a numeric variable.
* Set Variable Value no longer changes the type of the target Variable.
* Disabled Collapse / Expand All options on Parameters context menu when set to user mode.
* Improved drawing of Forces, whose gizmos can now be enabled or disabled.
* Numeric values in actions, conditions, variables and parameters can now be modified by dragging the mouse over.
* Replace In String no longer outputs the resulting string to an ouput variable, but replaces the specified target variable.
* Visual adjustments (separator lines, localization item background, drag and drop and block insertion indicator colors, condition fields, output labels, ... ).
* For Each now supporting the specification of the order of iteration (from first to last or from last to first).
* Temporarily disabled drawing of labels in Forces, Rays and Path gizmos due to a Unity bug which causes gizmo lines to be misaligned in Game view.
* Fixed issue whereby duplicates of a Comment actions were unmodifiable.
* Fixed foldout of disabled actions not working.
* Fixed rare error in console when changing GameFlow preferences.
* Fixed popup labels not using bold font on prefab override.
* Fixed GameFlow context help not showing when the project path contains extended-ASCII chars.
* Fixed default State colors.
* Fixed duplication of exception logs on actions executed in Editor time.
* Fixed Insert Item In List not inserting at index zero when list is empty (thx @V4lv1k!).
* Fixed On Collision / On Trigger programs only working for the first Collider in the target GameObject.
* Fixed collider validation in On Collision / On Trigger programs to no longer require a Rigid Body as mandatory.
* Fixed criterion for selecting where to add new actions / conditions when using the "Add Component" button in the Inspector. New actions and conditions will be now added to the last non-collapsed suitable container. If no expanded container is found, a new one will be created. The non-collapsed filter applies only to blocks, not to Programs nor State Machines.
* List Count condition is now obsolete, use Get List Count action instead.
* Evaluate is now obsolete, use Evaluate Conditions instead.
* Fixed localized warning text not properly displayed on the console.
* Added new prefab Score.
* Added example "Basic/B16 - Score" which shows how to use of the new Score prefab.
* Added example "Intermediate/I08 - Raycasting" which shows how to use the new raycasting support.

[0.9b]
* IMPORTANT: this is the last version of GameFlow supporting Unity 4.x, from now on, GameFlow requires Unity 5.
* You can now create State Machines easily by adding State components. States work like regular Programs but only the current state in each State Machine is executed at a time.
* New On State flow control action (works like a "If Current State is <State>"). Use it in event programs to execute actions only for specified states.
* New Get / Set Current State actions.
* New Current State condition.
* New Overview Component allowing to create a "navigation map" of the scene by defining links to specific GameObjects in the scene with a title and optional description. Supports localization.
* New Overview Window which shows the first Overview defined in the scene. Very useful to understand at a glance how a scene works.
* Added support for Commands. Now you can associate Command identifiers with Programs and execute them using the Execute / Send Command actions.
* New Destroy GameObjects In List action.
* New Copy Transform action.
* New Enable / Disable Components In List actions.
* New Send Command action. Executes the Command with the specified id.
* New Execute Command action. Executes the specified Command.
* New Wait For Program action.
* New Set / Get AudioSource Property actions.
* New Get Transform Point action. Converts a local Transform position into world position.
* New Get Screen Point From World Point action.
* Added State Machines example.
* Added Align UI example.
* Invoke Method no longer forces the use of variables to pass values to the method parameters, you can now use direct values (and *optionally* variables).
* Invoke Method action now supporting also static methods.
* Fields in inspector showing object references will no longer display the name of the currently inspected GameObject, but only the object identifier / type.
* Rewording of titles and properties of all the Condition blocks.
* Fixed bug preventing the Play Animation action to work.
* Fixed animation cross fading in Play Animation and Set Animator State being skipped sometimes.
* Added "Restart" option to Set Animator State and Play Animation (for Animator) actions.
* Fixed Key Condition not getting the first "Key Just Up" event when quickly pressing and releasing a key.
* Fixed potential problem with time-based actions executing in the Else section of an IF.
* Lots of minor visual improvements.

---

(c) 2016 Evasion Games, S.L.
