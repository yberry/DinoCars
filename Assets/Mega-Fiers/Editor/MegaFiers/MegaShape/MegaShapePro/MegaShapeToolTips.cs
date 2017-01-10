
using UnityEngine;
using UnityEditor;

public class MegaToolTip
{
	static public GUIContent Flip = new GUIContent("Flip", "This will invert the face normals for the mesh causing the mesh to turn inside out");
	static public GUIContent LayerName = new GUIContent("Name", "Name for the layer, other layers allow the selection of lofts as a surface so naming makes them easy to pick");
	static public GUIContent Enabled = new GUIContent("Enabled", "Turn the creation of this layer on or off. You could for example add a lot of layers to the loft and then depending on some game condition turn some off or on");
	static public GUIContent ParamCol = new GUIContent("Color", "Add a tint this layers params in the inspector making it easier to find");
	static public GUIContent Lock = new GUIContent("Lock", "If you lock a layer then it will not be recalculated if the layer is rebuilt, the layers last vertices and faces will just be added in, this can be used to speed up mesh generation on very complex lofts where you are only adjusting one layer");
	static public GUIContent Material = new GUIContent("Material", "The material this layer will use for rendering");
	static public GUIContent Path = new GUIContent("Path", "The path/shape to use to loft along, if you pick a shape that has multiple splines a slider will appear allowing you to pick which one to use");
	static public GUIContent Curve = new GUIContent("Curve", "The curve number to use in the selected shape");
	static public GUIContent Start = new GUIContent("Start", "The position along the selected shape to start the layer creation, 0 would be the start, 1 would be the end. On closed shapes the loft will wrap around if the value is outside the 0 to 1 range, if the shape is open then the loft will extend from the ends. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent Length = new GUIContent("Length", "The length of the loft along the selected shape, 1 would be a loft along the entire length of the path if the start value was 0. On closed shapes the loft will wrap around if the length plus the start value is outside the 0 to 1 range, if the shape is open then the loft will extend from the ends. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent Dist = new GUIContent("Distance", "The distance between each row of vertices in the loft, the smaller this value is then the more vertices that will be generated making a smoother surface");

	static public GUIContent UseOffsetX = new GUIContent("Use X Offset", "You can enable extra curve based offset for the X direction");
	static public GUIContent UseOffsetY = new GUIContent("Use Y Offset", "You can enable extra curve based offset for the Y direction");
	static public GUIContent UseOffsetZ = new GUIContent("Use Z Offset", "You can enable extra curve based offset for the Z direction");

	static public GUIContent OffsetCrvX = new GUIContent("Offset X", "This is the curve to control the X offset along the length of the loft. The curve relates to the entire length of the loft spline not the loft length");
	static public GUIContent OffsetCrvY = new GUIContent("Offset Y", "This is the curve to control the Y offset along the length of the loft. The curve relates to the entire length of the loft spline not the loft length");
	static public GUIContent OffsetCrvZ = new GUIContent("Offset Z", "This is the curve to control the Z offset along the length of the loft. The curve relates to the entire length of the loft spline not the loft length");

	static public GUIContent CrossParams = new GUIContent("Cross Params", "Open the inspector that has all the params for the cross section part of the loft");

	static public GUIContent Section = new GUIContent("Section", "The shape to use as the cross section for the loft. If the shape has more than one curve a slider will appear allowing you to choose which curve from the shape to use");
	static public GUIContent Snap = new GUIContent("Snap", "Set this to snap all the curves so that the first knot of each is in the same local space, use this if the curves in a shape arnt aligned");

	static public GUIContent CrossStart = new GUIContent("Cross Start", "The position along the selected cross section shape to start the layer creation, 0 would be the start, 1 would be the end. On closed shapes the cross section will wrap around if the value is outside the 0 to 1 range, if the shape is open then the cross section will extend from the ends. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent CrossLength = new GUIContent("Cross Len", "The length of the cross section along the selected shape, 1 would be a cross section using the entire length of the cross section if the start value was 0. On closed shapes the cross section will wrap around if the length plus the start value is outside the 0 to 1 range, if the shape is open then the cross section will extend from the ends. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");

	static public GUIContent CrossDist = new GUIContent("Cross Dist", "The distance between each vertex in the cross section, the smaller this value is then the more vertices that will be generated making a smoother surface");

	static public GUIContent IncludeKnots = new GUIContent("Include Knots", "If this is on then knots on the spline will be included in the cross section, this is useful to maintain the shape of the cross section more closely to the spline for lower detail levels, and for keeping detail and sharp corners etc");

	static public GUIContent UseCrossScale = new GUIContent("Use Scale Curve", "Allow the use of a curve for the scaling of the cross section along the length of the loft. The curve relates to the entire length of the loft shape not the actual loft");
	static public GUIContent CrossScaleCrv = new GUIContent("Scale", "This is the curve to control the cross section scaling along the length of the loft. The curve relates to the entire length of the loft spline not the loft length");

	static public GUIContent CrossOff = new GUIContent("Offset", "This value can be used to control the position of the cross scaling curve along the loft");
	static public GUIContent SeperateScale = new GUIContent("Seperate Scale", "Enables having seperate scale curves for the cross section X and Y");

	static public GUIContent UseCrossScaleY = new GUIContent("Use Scale Curve Y", "Allow the use of a curve for the scaling the Y axis of the cross section along the length of the loft. The curve relates to the entire length of the loft shape not the actual loft");
	static public GUIContent CrossScaleCrvY = new GUIContent("Scale Y", "This is the curve to control the cross section Y Axis scaling along the length of the loft. The curve relates to the entire length of the loft spline not the loft length");
	static public GUIContent CrossOffY = new GUIContent("Offset", "This value can be used to control the position of the cross Y Axis scaling curve along the loft");

	static public GUIContent AdvancedParams = new GUIContent("Advanced Params", "Opens the advanced params options for the loft");

	static public GUIContent SnapTop = new GUIContent("Snap Top", "This will adjust the loft so it is stretched so that the top of the loft will lay be of the Top Value");
	static public GUIContent Top = new GUIContent("Top", "The value the top of the loft will use");

	static public GUIContent SnapBottom = new GUIContent("Snap Bottom", "This will adjust the loft is stretched so that the bottom of the loft will lay be of the Bottom Value");
	static public GUIContent Bottom = new GUIContent("Bottom", "The value the bottom of the loft will use");

	static public GUIContent ClipTop = new GUIContent("Clip Top", "This will clip any points above the Clip Top Val to that value so causing the top of the loft to be flattened");
	static public GUIContent ClipTopVal = new GUIContent("Clip Top Val", "The value above which any vertex will be clip to");

	static public GUIContent ClipBottom = new GUIContent("Clip Bottom", "This will clip any points below the Clip Bottom Val to that value so causing the bottom of the loft to be flattened");
	static public GUIContent ClipBottomVal = new GUIContent("Clip Bot Val", "The value below which any vertex will be clip to");

	static public GUIContent UVParams = new GUIContent("UV Params", "Opens the UV mapping options");
	static public GUIContent UVOff = new GUIContent("UV Offset", "The Offset into the texture map to start the mapping from");
	static public GUIContent UVScl = new GUIContent("UV Scale", "The Scaling of the UV coords, smaller values will zoom in on the texture map, values greater than 1 will start to tile the texture, negative values will flip the texture");
	static public GUIContent UVRot = new GUIContent("UV Rotate", "Rotate the UV coordinates by this angle in degrees");
	static public GUIContent SwapUV = new GUIContent("Swap UV", "Will swap the u and v parts of the mapping");
	static public GUIContent PhysicalUV = new GUIContent("Physical UV", "If this is on then the actual vertex values will be used for the mapping, this can make it easier for some mapping cases it will also mean that the uv mapping will keep the scale of the texture map constant, so if you expand a loft the texel size will remain constant");

	static public GUIContent CapParams = new GUIContent("Cap Params", "Opens the End Capping options");
	static public GUIContent CapStart = new GUIContent("Cap Start", "Allows a mesh cap for the start of the loft mesh to be generated");
	static public GUIContent CapStartMat = new GUIContent("Material", "Material to use for the start mesh cap");
	static public GUIContent CapStartRot = new GUIContent("Rotate", "Angle to rotate the start cap UV's by");

	static public GUIContent CapEnd = new GUIContent("Cap End", "Allows a mesh cap for the end of the loft mesh to be generated");
	static public GUIContent CapEndMat = new GUIContent("Material", "Material to use for the end mesh cap");
	static public GUIContent CapEndRot = new GUIContent("Rotate", "Angle to rotate the end cap UV's by");

	static public GUIContent UseTwist = new GUIContent("Use Twist", "Enabled the twist amount and curve values for twisting the mesh along the length of the loft");
	static public GUIContent Twist = new GUIContent("Twist", "The amount to multiply the Twist Curve value by to get the total amount of twist.");
	static public GUIContent TwistCrv = new GUIContent("Twist Crv", "This curve controls the amount of twist along the length of the loft, 0 means no twist");

	static public GUIContent Layer = new GUIContent("Layer", "The Layer on the Loft object to build this layer onto");
	static public GUIContent Surface = new GUIContent("Surface", "The Loft object to build this layer onto");

	static public GUIContent StartSurface = new GUIContent("Start", "The position along the selected surface to start the layer creation, 0 would be the start, 1 would be the end. On closed shapes the loft will wrap around if the value is outside the 0 to 1 range. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent LengthSurface = new GUIContent("Length", "The length of the layer along the selected surface, 1 would be a layer along the entire length of the surface if the start value was 0. On closed surfaces the loft will wrap around if the length plus the start value is outside the 0 to 1 range, if the shape is open then the loft will extend from the ends. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent CrossStartSurface = new GUIContent("Cross Start", "The position across the selected surface to start the layer creation, 0 would be the start, 1 would be the end. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
	static public GUIContent CrossLengthSurface = new GUIContent("Cross Length", "The length of the layer across the selected surface, 1 would be a layer across the entire surface if the start value was 0. On closed surfaces the layer will wrap around if the length plus the start value is outside the 0 to 1 range. The range of this slider is set by the limits value in the Mega Shape Loft Limits values");
}
