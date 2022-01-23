% Define the grain directory
grainDirectory = 'Example Output';

% Create the grain pack
grainPack = GrainPack(grainDirectory);

% Convert to binary Image (resolution is just the delta distance between
% each voxel)
grainPack.createBinaryGrainPack(.03);

% Extract subvolume (percentages cut from each dimension. Each dimension
% can have one value for each direction or two values that are different
% for each direction, e.g., down and up in the Z direction in the example
% below)
grainPack.extractSubVolume(.1,.1,[.05 .2]);

% Visualize 3D (slow), don't use on large images
% grainPack.visualize3D();

% Visualize slices (fast). The slice percentage is defined in each 
% direction

figure('Color', 'White')
subplot(1,2,1)
grainPack.visualizeSlices(.5, .5, .5)
subplot(1,2,2)
grainPack.visualizeSlices([0,1], [0,1], [0,1])

% Get the binary image and typed image from the object
bwImage = grainPack.getBinaryImage();                         % Binary image
labeledImage = grainPack.getLabeledImage();                   % Grain number image
[typedImage, typeInfo, nTypes] = grainPack.getTypedImage();   % Numbers based on grain type 

% Reset the cropping
grainPack.resetSubVolume();

% Operation on one grain example (grain index 10), get curveture
% measurment
grainIndex = 10;
grain = grainPack.grains(10);
Vertices = grain.getFV().Vertices;
Facies   = grain.getFV().Faces;
[Cmean,Cgaussian,Dir1,Dir2,Lambda1,Lambda2]=patchcurvature(grain.getFV(),true);
[sphericity, volume, surfaceArea] = grain.calculateSphericity();

