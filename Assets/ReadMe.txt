
****************************************
Overview
****************************************

We have all seen game with beutiful rolling backgrounds, now you can have them too!

Making a racing game? Then no problem with Mega-Shapes. Draw a path for the road, draw a cross section and then loft a road. Want more detail? Then Loft a barrier and it will automatically conform to the road surface. Slide the barriers in or out, scale them, twist them, you imagination is the only limitation.

Want more? Then use the powerful rail clone feature to repeat and position sets of objects along your road, fences, powerlines, trees whatever you like.

Still not enough? For those that want even more Mega-Shapes gives you a scatter system that will take any objects you choose and scatter them along your road and have them auotmatically conform to the existing meshes surface.

With Mega-Shapes you can create beutiful levels quickly and easily.


MegaShapes is a system that builds meshed from splines, this can be anything from simple lines, ribbons using shapes as the paths to complex lofts of multiple spline cross sections along a spline path. Combined with this is an advanced layer system for adding deltails to the lofts, with layers that allow cloning of objects, as well as scattering objects over a loft to a full rule based system for building detailed meshes along a path or mapped to a loft surface.

All aspects of the system can be edited in realtime with layers conforming automatically to any changes in the base shapes or surfaces allowing for very easy editing and updating.

MegaShapes current features:
Full spline bezier based spline system.
Turn shapes into meshes with uv mapping and extrusion options
Controllers for moving objects along paths
3ds max exporter to get paths and animated paths easily into the system (soon to be extended to other editors as well as support for vector based files)
Simple loft system to extrude any shape along another to build roads, tubes, surfaces etc
Advanced loft system to allow any number of cross sections to be used to build a loft surface
Loft layers to add further detail to surfaces, layers currently included (more to come):
	Clone - Take any mesh and duplicate it along a surface with options for start and end meshes
	Clone Spline - Clone a mesh along a shape again with start end mesh options
	Scatter - randomly scatter meshes onto a loft surface
	Scatter Spline - randomly scatter meshes along a spline
	Clone Rules - Adanced rule based mesh builder, current rules are:
		Start	- Object to be used at the start
		End		- Object to be used at the end
		Filler	- Main filler object
		Regular	- Object to appear every Nth object
		Placed	- Object to appear at specific location
		
		You can have any number of rules and where more than one of the same type is found a weighting value is used to choose which to use.
	Clone Spline - Like above but along a spline.

Features to come in updates:
Fast Rope and chain system
Scatter objects inside splines
Animated breaking Wave simulator
Move loft layers and enhancements to the current system
Move rules for the rule based layers

All code is copyright Chris West 2015 and any use outside of the Unity engine is not permitted.

****************************************
How to use:
****************************************
More information on the system can be found on the website at http://www.west-racing.com/mf

****************************************
Changes:
****************************************
v2.21
MegaShapes fully Unity 5.5 compatible.
Layer field for the Rope collision field now shows all the layers correctly.
Fixed bug with rope collisions not using the selected layer value correctly.
Moved the New Hose system source file from the editor folder to the correct folder. You may need to delete the MegaHoseNew.cs file in the editor folder after an update.
Added new Hose Attach system that uses the new hose system.

v2.20
Added a new version of the hose system that works correctly when end nodes are transformed or moved. Use 'Hose New' instead of 'Hose'.

v2.19
Optimized the Lathe mesh builder.
Improved the Lathe Normal calculation system so it works correctly for all cases.
Fixed the mesh array size errors in the Lathe system when reducing the vertex count.
Fixed the Regex error in the SVG importer when building for a Windows Store App
Editing of splines is now disabled if an Alt key is down, stops accidental editing of splines while changing the scene view.

v2.18
Added simple demo scene showing the RBody following a path component.

v2.17
Fixed error with Spline Fill mesh when being used as a collider with zero thickness.

v2.16
MegaShapes fully compatible with Unity 5.3

v2.15
Train demo scene fixed so the train works correctly in Unity 5.x
Fixed main demo scene where one of the lofts had its rules disabled causing an exception.

v2.14
Fixed a problem in Unity 5.x where zero length tris would cause an error and stop the collider being built.

v2.13
Fixed exception in inspector if you happen to create a spline with no knots in it.
Added option to turn of the autosmooth when a new knot is added to the spline.
Also added the AutoCurve option when the Add Knot button is clicked
Fixed bug in the Lathe system where normals were pointing the wrong way

v2.12
Fixed exception bug in the spline notify system that could appear when loft layers which do not use crosssections are used.
Added conform option to the Clone spline layer so can now easily make fences etc fit the terrain
Added conform option to the Clone spline Rules layer so can now easily make fences etc fit the terrain
Added conform option to the Clone spline Simple layer so can now easily make fences etc fit the terrain
Added conform option to the Scatter spline layer so can now easily make fences etc fit the terrain

v2.11
Changes for Unity 5.1, removal of obsolete items etc.
Fixed CrossSection lines not showing for Complex layer type.

v2.10
Fixed MegaShapeLightMapWindow problem when doing an app build.
You can now build lightmap data for MegaShape standard meshes.
Changed OSM importer to use ulong for ids instead of int so can now handle complex OSM files.

v2.09
Added a new demo scene to show a RBody being controlled by a spline. Car model kindly provided by www.uistudios.com

v2.08
Fixed Shape labels being displayed if behind the camera.
Added a beta OSM data importer. Click the Assets/Import OSM option.

v2.07
Added example script showing how to move a RigidBody object along a spline with forces.
Added FindNearestPointXZ and FindNearestPointXZWorld methods to MegaShape API.
Improved Move RigidBody along spline system example script MegaShapeRBodyPathNew, note it is an example script for you to base your own on.
Added a break force option to RigidBody spline follow.
Added more options to the Move Rigidbody along a spline script.

v2.06
Changed Clone Spline simple so position of layer is not linked to spline position, no need to duplicate the spline if used multiple times.
Changed Clone Spline so position of layer is not linked to spline position, no need to duplicate the spline if used multiple times.
Changed Clone Spline Rules so position of layer is not linked to spline position, no need to duplicate the spline if used multiple times.
Fixed exception if trying to build a mesh with a shape with no splines.
Fixed exception if trying to interpolate along a shape with no splines.
Fixed exception if trying to interpolate a spline with no knots.

v2.05
Remove the SetActive warning from Draw Loft Component.
Added in the Mega Mesh Page component.

v2.04
Added a new Scatter system (MegaLoftScatter) to allow you to scatter complete objects onto lofts.

v2.03
Tweaks to the Draw loft and Draw Spline components.

v2.02
Autocurve fixed so the last knots handles on an open spline are correct.
Added a Draw Spline example script to make splines by drawing with the mouse at runtime.
Added a Draw Loft example script to make new lofts by drawing with the mouse at runtime.
Added a demo scene showing the draw spline system.
Added draw loft example to the first tutorial scene.
Fixed some potential errors in the constant speed interpolation.
Smooth value is now a slider and shows results in realtime for easier use.

v2.01
Fixed Terrain Carve gizmo being offset from the loft.
Fixed exception when first adding a terrain object to the terrain carve system.
Made the default curves for the terrain carves correct for left and right.
Terrain carve now works with open lofts.
Terrain carve outline splines now use same interpolation settings as the loft spline for better results.
Terrain carve Conform button will not appear in the inspector until the Save Heights button has been clicked to save confusion.
Fixed bug in the WalkLoft GetLocalPoint method where it was not using the passed dist value.
Fixed bug in the WalkLoftSmooth GetLocalPoint method where it was not using the passed dist value.
Added GetPoint method to WalkLoft to get any point on the loft, as opposed to the GetLocalPoint which is relative to the current walk loft position.
Added GetPoint method to WalkLoftSmooth to get any point on the loft, as opposed to the GetLocalPoint which is relative to the current walk loft smooth position.

v2.00
Further small changes to make compatible with Unity 5.0

v1.98
Fixed import warnings for the latest Unity 5 beta.

v1.97
The Clone Spline layer now works with the spline twist values.
Fixed exception when adding a new curve to a shape.
Added option to Spline Tube Mesh to flip normals for inside tubes.
Added option to Spline Box Mesh to flip normals for inside box tubes.
Added option to Spline Ribbin Mesh to flip normals.
Added a method GetLocalPoint(float dist, float crossa) to WalkLoft script to allow you to easily get a point on the loft in the local space which can be used for steering controls etc.
Added a method GetLocalPoint(float dist, float crossa) to WalkLoftSmooth script to allow you to easily get a point on the loft in the local space which can be used for steering controls etc.

v1.96
Imported SXL splines will now no longer change values to centre the spline.
Imported SVG splines will now no longer change values to centre the spline.
Added Centre Shape button to Shapes Inspector to allow you move the pivot to the centre of all the points.

v1.95
Fixed a bug in the MegaShapeFollow script when using the option to rotate objects to align to the spline.
Added new InterpCurve3D method which will return the postion, twist and also rotation quaternion for a point on a spline.

v1.94
Fixed exception in Terrain Carve if no Loft Layer selected.
Fixed exception if you loaded a spline file with fewer splines in it to a shape that was being used by a loft that used a deleted spline 

v1.93
Fixed an exception bug when duplicating layers.
Updated code for Unity 5.x
Autocurve now does the first and last handles on open splines.

v1.92
Small fix for Unity 4.x where loft layers were not updating when a spline was edited.

v1.91
Fixed Path Follow script so it works correctly with spline twist.
Fixed Path Follow script so offset and extra rotation work correctly in all cases.
Added 'Update on Drag' option to MegaShape inspector, if checked spline meshes or lofts will update as you drag, off then they will update when dragging stops, for complex lofts this makes it easier to edit splines.
Fixed issues when creating a Clone Spline simple layer from the popup window. ie path and objects not being set
Fixed inspector for spline animations so buttons aren't hidden.

v1.90
Fixed the issues with using Multi Mat Complex layers as base layers for other layers such as clones, cross alpha values were not being calculated correctly.
Fixed Multi Mat Complex layer where too many vertices and polygons were being created, such lofts will be more optimized now.
Fixed issue where Multi Mat Complex layer would not give correct up values.

v1.89
Fixed exception when using other loft layers with Multi Mat layer when cross alpha values were out of range.
Fixed exception when using other loft layers with Multi Mat Complex layer when cross alpha values were out of range.
Walk Loft Smooth script now works with the Multi Material Complex loft layer.

v1.88
Added SVG option to export MegaShape splines to SVG files.

v1.87
Fixed warnings about obsolete methods in Undo system or Unity 4.5 and 4.6
Fixed bug in Walk Loft Smooth for open lofts when alpha value is 1.0
Fixed exception error in Multi Mat Complex layer.

v1.86
Conform now works in the Multi Mat Complex layer.
Fix bug when conforming simple mod layer with capped ends causing excetion in some cases

v1.85
Fixed bug in Multi Mat Later where last vertex wasn't being added if cross distance value was large.
Duplicate layer now works for Multi Mat layer
Duplicate layer now works for Complex Multi Mat Layer

v1.84
The gizmos for Complex Loft now align correctly if twist mode active.
Added beta of the new Multi Material Complex loft layer.
Copy Knot IDs function added to extra functions section of Shapes inspector. Copies ids from the previous spline in the shape.
Terrain carve works with new Multi Mat complex layer
Terrain carve left and right scale values now dont effect one another.
Fixed a bug when Calc up was on on Clone Rules layer causing meshes to deform.
Knot index now shown in the inspector for easier editing.
Toogle added for drawing the orgin handle that moves all the knots.
Added snap option when positioning knots
Added snap option when positioning knot handles.

v1.83
Beta of the Loft Terrain Carver added.
Fixed error message when deleting a layer from a loft.
Changing a spline used in a loft will now automatically update that loft, no longer need the realtime option set
Added KML import of splines
Offset value in Path Follow now works correctly

v1.82
Multi Mat Layer include knots bug fixed.
Track demo scene improved.

v1.81
Added beta of new Multi Material Layer
Added new race track demo scene
Fixed twist angles not being calculated correctly for out of range values on closed splines.
When adding a new knot it will now use the ID and Twist value from the previous knot
Added a Flatten button to the inspector so you can quickly flatten any spline
Added a Remove twist button to the inspector to reset any twists on a spline
Added a SetHeight method to the API so you can quickly set all knots and handles for spline to the same height
Added a SetTwist method to the API so you can quickly set all the knots in a spline to the same twist angle
Added option to choose easing mode for twist values, either Linear or Sine at the moment.
Added id values to the spline knots.
Added beta of new Loft Layer, this uses the new knot id values to assign different materials to the cross section, this allows a simple loft to easily have different materials applied.
Context Menum Help now works for Complex and Multi Mat layers
Demo scenes tidied up

v1.80
Fixed the Create Layer popup window so the params are correct for all layer types now.
You can now select the spline to use in the Complex loft layer when the path shape has more than one spline.
Walk Loft smooth works correctly when loft uses a curve value other than 0
Walk Loft works correctly when loft uses a curve value other than 0
Curve value now works correctly on Clone Spline Rules layer
Fixed end caps being one tri short in some cases on simple loft layer
Fixed bug causing extra un needed vertex being added per cross section in simple loft layer

v1.79
Fixed bug where a duplicated layer was not showing up in the until its vertex count changed.
Fixed the exception that was reported when layers were copied.
Fixed bug where you could not set the init values in the layer create window for simple and complex lofts.

v1.78
Complex loft now works with knot twist values.
Clone Spline Simple layer now works with knot twist values
Clone Spline layer now works with knot twist values
Clone Spline Rules layer now works with knot twist values
Scatter Spline layer now works with knot twist values
Walk Loft smooth now works with complex lofts as well.

v1.77
Walk Loft smooth bug fixed where position was wrong if loft not at 0,0,0
Walk Loft smooth now works with the twist values

v1.76
Changed the knot twist handle to make it simpler and less cluttered.
Fixed a slowdown that could happen with twist handles enabled.

v1.75
Added beta support for twist values per knot, currently works for Simple Loft Layer.
Knot twist value now used in the extruded mesh options box, tube and ribbon
Path Follow script can use knot twist values
Character Follow script fixed.
New methods in API to Interpolate and get twist along with position

v1.74
Added a BuildSplineWorld method to the api so splines can be built from world space points.
Fixed a bug in the Simple Loft layer where the cross section vertices were not calculated correctly.
Added another option to the Simple Layer UV params, 'Calc Y' will work like Phys UV but keep uv x between 0 and 1 and calc y based on length.
Added a beta of a new path follow script that will allow characters to be constrained to a path but otherwise be allowed to move freely and jump etc while on that path MegaCharacterFollow.cs

v1.73
Fixed bug when constant speed opened splines are reversed.

v1.72
Added loop mode to Path Follow script so you can pick from Clamp, PingPong, Loop or none.
Added loop mode to Shape Follow script so you can pick from Clamp, PingPong, Loop or none.
Fixed a bug where constant speed interpolation was not being used for open splines.
Added the ability to create animated splines inside Unity, you can now add keyframes for splines and have them played back for you.

v1.70
Added MoveSpline methods to API for easy moving of entire spline
Added RotateSpline methods to API for easy rotation of entire spline
AutoCurve Button now only updates the current spline being edited
Added Gizmo to allow current spline to be moved in the editor for easy repositioning
Fixed inpsector issue when selecting a new loft in some components
Added MegaWalkLoftSmooth which moves objects on lofts perfectly smoothly
Made it easier to add knots to a spline, click and drag the mid point circle to add a new knot at that position

v1.69
Update to the SVG importer to handle path 'm' commands with multiple values ie fill a shape.
Improved peformance of spline interpolation when using constant speed.
Constant speed defaults to Normalized Interp for interpolation.

v1.68
Added new transport frame method to simple loft layer allowing for loops to be created with no ugly twists
Added new transport frame method to complex loft layer allowing for loops to be created with no ugly twists

v1.67
Performance increase with the loft system, moved check for layers so not happening everyframe.
Removed unused Update methods
Added flipcap option to Simple Loft so you can flip the endcaps if used separatley from the main loft
Added flipcap option to Complex Loft so you can flip the endcaps if used separatley from the main loft

v1.66
Added option to MegaShapeLoft inspector to disable Undo checks, some 4.3 users have reported slowdowns in the Undo system in 4.3

v1.65
Fixed exception in MegaShapeLoft Start which could happen when creating a new Loft
Added MegaPathFollow, which is a simple spline following helper script.

v1.64
Fixed errors in undo system.

v1.63
Added a 'smoothness' value to MegaShape which is used by the AutoCurve method, a value of 0 will result in sharp corners, a value of 2 will give very wide entries into knots, a value of 1.1 seems to be the best value for smooth curves.
Unity 4.3 new undo support.

v1.62
Train follow has debug lines for easier editing.

v1.61
Fixed bug in Clone Spline Simple where axis value was being used correctly to calculate repeat count.
Fixed bug in Clone Spline Simple where loft was not being positioned correctly over spline if spline not at 0,0,0
Fixed bug in Clone Spline where loft was not being positioned correctly over spline if spline not at 0,0,0
Added helper script MegaTrainFollow that allows for multiple objects to follow eg like a train with carriages

v1.60
Conform added to fill spline meshing option.
Conform added to box spline meshing option.
Conform added to tube spline meshing option.
Conform added to ribbon spline meshing option.

v1.59
Added beta of new Conform feature which allow lofts to conform to a mesh or terrain.
Fixed bug when creating lofts from script where an invalid index exception was being generated, thanks to FredPointZero

v1.58
Added Align Cross slider tp Simple Loft layer to define how much the cross section aligns to the path spline, similar to the teeter value in the complex loft.

v1.57
MegaGrab removed if platform is not webplayer or standlone.
Fixed warnigns that have appeared in Unity 4.x

v1.56
Fixed a bug that give exceptions if you used scatter layers with a complex loft.

v1.55
Made changes for Unity 4.2 onwards that stopped you being able to select layers correctly in the inspector.

v1.54
Changes to make Windows App and Phone compatible

v1.53
Fixed normals not flipping correctly on Lathed meshes.
Fixed smoothing on last cross section points on lathed closed splines.

v1.52
Fixed bug in walk loft if Lateupdate option used.

v1.51
Added MoveKnot() method to megashapes.
Added animate option to walk loft to aid moving objects.
Added option to control how upright objects remain when using walk loft system.
MegaShapeFollow distance values now repeat if they go out of range.
Fixed Lathe to work correctly with closed splines.

v1.49
Update to the Mega Prefab system, will attempt to reconnect any shapes used by a loft to shapes in the scene.
Fixed a bug when selecting layers which could give a casting error depending on order layers are applied to a loft.

v1.48
Added MultiBlend Diffuse Shader for use with the new vertex coloring options.
Added MultiBlend Specular shader for use with the new vertex coloring options.
Added vertex color option to Simple Loft layer, this can be use with the new shaders to blend between textures along the loft.
Fixed zero vector3 issue in scatter simple layer.
Create MegaShape Prefab fixed to work properly.

v1.47
Added side view option to Planar mapping mode on Simple Loft layer making it even more useful for side view games.
Added option to lock the world coordinate used for the world planar mapping mode on the simple loft layer.

v1.46
Mode option added to Planar mapping to use world or local space values to control planar mapping as well as normal mode.

v1.45
Fixed error if Mega Shape component added directly to a game object.

v1.44
Planar UV Mapping option added to the Simple Loft Layer.
Beta of Lathe Spline system added.
Added scaling option to tube meshing params so you can easily make tubes that get fatter or thinner
Added scaling option to box meshing params so you can easily make box tubes that get fatter or thinner
Added scaling option to ribbon meshing params so you can easily make ribbons that get wider or thinner

v1.43
Chain building option now works in the rope and chain system (still in beta)
Chain example added to Rope Scene
MegaGrab updated to the latest version

v1.42
Added basic Line Shape to the helper shapes so you can create a simple straight line spline and set the number of points as a useful starting spline.
Added Beta of Constant Speed option to splines, you can choose per spline if it is a constant speed bezier spline or a normal bezier spline.
Calc Subdivs value added this will make length calculations more accurate the higher the value, will also make the constant speed interpolation more accurate. This can be set per spline.
Tank Track help page added.
Hose help page added.

v1.41
Twist curve added to simple loft, useful for doing banked corners etc
Option added to Simple Loft to Reset curves so there is a keyframe for each knot on the spline, useful when using twist curves etc.
Fixed bug which showed Duplicate and Delete buttons as greyed out on Scatter layers.
Updated MegaShapes docs
Added docs for the meshing options for MegaShapes.
Added methods to ShapeFollow to allow easy creation of follow targets and getting targets so the weights etc can be adjusted.

v1.40
Removed MegaMOrbit file which caused an error if MegaFiers and MegaShapes installed.
Fixed Mesh Fill option not working correctly if shapes were generated with an axis value or X or Z.
Fixed errors on 4.0 and 4.1 for SetActiveRecursively

v1.39
Cursor position is now a per spline value instead of a single static.
Fixed problem with layers extending beyond surface loft wrapping around causing meshes to stretch etc, will now work correctly.
Added option to Simple Loft layer to choose the origin of the UV, either from start of loft spline, or start of the loft.
Added option to Complex Loft layer to choose the origin of the UV, either from start of loft spline, or start of the loft.

v1.38
Updated BezFloatKeyControl to match the new MegaFiers version.
Added Do Late Update option to Hose system
Added option to disable hose updates if not visible
Added Tank Tracks System
Added Tank Wheels helper system

v1.37
Hoses named correctly when created.
Fixed hose not initializing correctly on creation.
The hose system now works correctly with prefabs.
Hose Freecreate mode disables automatically if end objects defined.

v1.35
Replaced use of GameObject.active with GameObject.SetActiveRecursively() to be compatible with Unity 4.0.1
Added Mega Hose system to MegaShapes, easily connect two objects with a flexible hose.
Added Mega Hose Attach to MegaShapes, attach objects to the hose.

v1.33
NormMap renamed to avoid naming clash.
MOrbit.cs script renamed to avoid errors if installed with MegaFiers

v1.32
Optimized the Verlet integrator.
Added MegaGrab to the package.

v1.31
Fixed bug in Helix shape with one of the knot handles being wrong.
Fixed exceptions in the alpha rope code
Added custom inspectors for the rope system.
Added a Verlet solver to the rope system.
Added first test of self collide to rope system.
Added start of a new scene for rope testing.
Added a new rope meshing option, deform mesh to rope spline.

v1.30
Added a Create MegaShape Prefab option to the GameObject menu so you can easily create prefabs out of lofts or shape meshes.
Removed some debug logging messages when loading SVG and SXL files.
Rope and chain code added in an alpha state ie not fully tested or with all features present

v1.28
Support added for SXL file import, sxl is a xml based spline format allowing users to easily write their own spline exporters.
Bezier curve Exporter for Maya available

v1.27
Up value used correctly for complex loft layers.
Editing of splines works correctly for scaled and rotated splines
SVG import now uses Axis value on import for orientation.

v1.26
Deleting a layer will now rebuild mesh if realtime is off.
Fixed strange results when typing in values for Offset values on Clone Layer.

v1.25
Added Ribbon option to mesh types when converting Spline to a mesh.
Fixed MeshRenderer being added to object when it wasn't required.
Added Late Update option to shapes.
Pivot offset value now works for tube, box and ribbon meshing options.
Added UV offset param to tube, box and ribbon meshing options.

v1.24
Walk Loft now has an alpha mode and a distance mode for positioning object.
Shape Follow now has an alpha mode and a distance mode for positioning object.
Added Late Update option to Walk Loft
Added Late Update option to Shape Follow
Added Help page for Shape Follow
Added Help page for Walk Loft
When loading SPL or SVG files you now have the option to Replace existing splines or add to the splines making the shape.
Add example script MegaMovePlayer.cs to show how to control character moving on a spline.

v1.23
Added Elipse support to SVG importer.
Fixed splines not showing up when selected in Unity 4.0
Added a Reverse spline option to the Shapes inspector, will reverse the currently selected spline
Added an Apply Scaling button to the inspector so if you scale the shape using the transform click this to correctly scale the splines
Added an Outline spline system, you can now ask the system to make a new spline that outlines the current one with control over the outline distance.

v1.22
SVG importer rewritten and greatly improved.
CursorPos on shapes now works correctly for the selected curve.

v1.21
Fixed an exception error if Complex Layer missing a path.
Complex Loft layer now handles non closed splines properly for our of range values ie alpha < 0 or > 1

v1.19
Fixed the flip not working on the Complex Loft layer.
Added error checking code to Complex Loft so check for valid state before trying to draw. NB Complex loft needs at least 2 cross sections before mesh builds.
Added start of SVG import support.
Optimized some of the core maths for faster updating of loft meshes, more to come.
Added a rotate value to the walk loft script.
Fixed bug in Duplicate Layer code, now works for all layer types.
Added an offset and modifier param to each target in Shape Follow.
Jump and tree assets added for turoial scenes

v1.18
Made some changes to how the twist works on tube and box meshes.
Fixed bug on Duplicate Layer which caused an exception.
Update help page for Loft Object component.
Changed how the system deals with transforms, now allows objects to be moved, scaled and rotated and layers will follow correctly (if realtime set), you may need to adjust rotations on clone layers and offsets depending on base object rotations etc. Please backup before updating.
When copying a Loft Object layers will now use the newly created Loft Surface instead of the original one if the Loft Object is the one being copied.

v1.17
Walk Loft works correctly with transformed loft objects.
Added limits for the 'Dist' and 'CDist' params to the Loft Object inspector.

v1.16
Added Box type to meshing options for splines.
Added a Vertical offset value to the Walk Loft helper script.
Fixed inpsector gui bug
Add Mega Shape Follow to allow gameobjects to follow a shape or multiple shapes.

v1.15
Added 'Duplicate' button to layer inspector to easily make a copy of a layer on a loft object.
Added 'Delete' button to layer inspector to easily remove a layer.
Added a smooth value to Walk Loft helper script.
Added 'tangent' value to Loft Object to control the accuracy of the forward calculations in the lofter.
Added new meshing option to splines, 'Tube' allows multistranded tubes to be made.

v1.14
Added ability to copy loft objects easily

v1.13
Added autosmooth option to shapes.
Added BuildSpline method.
Added various methods to aid in spline building via script.
Added option for different handle types on Shapes as some people reported big slowdowns in 3.5.3

v1.12
Adjusted sensitivity of some Editor GUI values

v1.11
Issues a warning if a Loft Object will generate too many vertices.
Fixed bug in uv mapping calculation on Simple Loft Layer.
Added a global Offset value to Clone Layer
Added a global Offset value to Clone Simple Layer
Added a global Offset value to Clone Spline Layer
Changed the up calculation to make it consitant between when up is calculated and when its not, may need to change your rotations if you see old clones rotated.
Fixed bug where you could close Main Params foldout

v1.10
Bug fixed of shapes not being created when active scene port wasnt selected.
Shape cursor position now respects the selected curve in multi curve shapes
Adjusted error checking.

v1.09
Bug fixed that stopped scatter layers moving if surface object rotated or moved.
Error checking when objects without meshes are selected added.
Added error dialog when trying to select objects with no meshes.

v1.08
Bug fix for Clone layer not being created from the Layer Create Window fixed.
Removed unused layer types.

v1.07
Fixed exception errors when copying loft objects between scenes, you will need to select paths again after copy.
Folldouts now have colored buttons to make it easier to see and use.
Rebuild Button on Loft object changed to a toggle value.

v1.06
Scatter Layers no longer update it their animation Speed is 0 or the Layer is disabled

v1.04
Added support for Lightmapping of finished loft Objects, a Build Lightmap button as been added to the Loft Object inspector.

v1.03
Modbut class renamed to MegaModBut to avoid naming conflicts.
Added missing button to create first cross section on Complex Loft.

v1.0
First release

