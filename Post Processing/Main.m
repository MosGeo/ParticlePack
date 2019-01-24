% Define main Directory
mainDirectory = pwd;
grainDirectoryName = 'Example Output';

% Load all the grains
[grains] = loadGrains(grainDirectoryName, mainDirectory);

% Create the grain pack
grainPack = GrainPack(grains);

% Convert to binary Image (resolution is just the delta distance between
% each voxel)
grainPack.createBinaryGrainPack(.03);

% Extract subvolume (percentages cut from each dimension. Each dimension
% can have one value for each direction or two values that are different
% for each direction, e.g., down and up in the Z direction in the example
% below)
grainPack.extractSubVolume(.05,.05,[.05 .1]);

% Visualize 3D (slow)
grainPack.visualize3D();

% visualize slices (fast). The slice percentage is defined in each direction
grainPack.visualizeSlices(.5, .5, .5)
grainPack.visualizeSlices([0,1], [1], [0])

% Get the binary image from the object
bwImage = grainPack.binaryImage;   % Cropped
bwImageFull = grainPack.fullBinaryImage;   % Not cropped

% Reset the cropping
grainPack.resetSubVolume();

