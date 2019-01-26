% Define main Directory
mainDirectory = pwd;
grainDirectoryName = 'Example Output';

[grains] = loadGrains(grainDirectoryName, mainDirectory);
grainPack = GrainPack(grains);
grainPack.createBinaryGrainPack(.03);
grainPack.extractSubVolume(.05,.05,[.05 .1]);

labeledImage= grainPack.labeledImage;
bwImage = grainPack.binaryImage;

figure('Color', 'White')
subplot(1,2,1)
ax1 = grainPack.visualizeSlices(.5, .5, .5, labeledImage);
subplot(1,2,2)
ax2 = grainPack.visualizeSlices(.5, .5, .5, bwImage);
Link = linkprop([ax1, ax2],{'CameraUpVector', 'CameraPosition', 'CameraTarget', 'XLim', 'YLim', 'ZLim'});
setappdata(gcf, 'StoreTheLink', Link);
colormap default
