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

% Get the binary image from the object
bwImage = grainPack.getBinaryImage();   % Cropped

% Reset the cropping
grainPack.resetSubVolume();