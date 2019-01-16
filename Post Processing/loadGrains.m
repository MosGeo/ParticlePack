function [grains] = loadGrains(grainDirectoryName, mainDirectory)

if exist('mainDirectory', 'var') == false; mainDirectory = ''; end
stlDirectoryName = '';
resultsFileName = 'Results.dat';

grainDirectory = fullfile (mainDirectory,grainDirectoryName);
grainsListing = dir(fullfile(grainDirectory, stlDirectoryName, 'Grain*.stl'));

nGrains =  numel(grainsListing);

% load Results File
nameColumn = 1;
positionColumns = [2 3 4];
rotationColumns = [5 6 7];
scaleColumns = [8 9 10];
formatSpec = '%s%s%s%s%s%s%s%s%s%s%s%s%s%s';
resultsFileName = fullfile(mainDirectory, grainDirectoryName, resultsFileName);
fileID = fopen(resultsFileName,'r');
dataArray = textscan(fileID, formatSpec, 'Delimiter', ' ', 'MultipleDelimsAsOne', true, 'HeaderLines' ,1, 'ReturnOnError', false);
fclose(fileID);
cell2num = @(inputCell) arrayfun(@(x) str2double(x), inputCell);
grainNames = dataArray{nameColumn};
grainPosition = [cell2num(dataArray{positionColumns(1)}) cell2num(dataArray{positionColumns(2)}) cell2num(dataArray{positionColumns(3)})];
grainRotation = [cell2num(dataArray{rotationColumns(1)}) cell2num(dataArray{rotationColumns(2)}) cell2num(dataArray{rotationColumns(3)})];
grainScale = [cell2num(dataArray{scaleColumns(1)}) cell2num(dataArray{scaleColumns(2)}) cell2num(dataArray{scaleColumns(3)})];
clear nameColumn positionColumns rotationColumns scaleColumns formatSpec fileID resultsFileName dataArray

% LOAD GRAINS
grains(nGrains) = Grain(nGrains);
tic
for grainNumber = 1:nGrains
    disp(['Loading Grain ' num2str(grainNumber) ' Out of ' num2str(nGrains)])
    selectedGrainName = grainsListing(grainNumber).name;
    grainNameExtract = strsplit(selectedGrainName, ' - ');
    grainNameExtract = grainNameExtract{2};
    [~, ind] = ismember(grainNameExtract(1:end-4), grainNames);
    grainFileName = fullfile(grainDirectory, stlDirectoryName, selectedGrainName);
    [vertices,faces,normals,~] = stlRead(grainFileName);
    
    grains(grainNumber).setMeshData(vertices, faces, normals);
    grains(grainNumber).setName(selectedGrainName);
    grains(grainNumber).setPRSdata(grainPosition(ind,:), grainRotation(ind,:), grainScale(ind,:))
    grains(grainNumber).translateGrain(grainPosition(ind,:));
end
clear selectedGrainName ind vertices faces normals grainPosition grainRotation grainScale 
toc

% CREATE GRAIN PACK
%grainPack = GrainPack(grains);
%grainPack.createBinaryGrainPack(.03);
%im = grainPack.extractSubVolume(.05,.05,[.05 .1]);
%grainPack.resetSubVolume();
%grainPack.visulize3D();
%stackFileName = [grainDirectory '\Image Stack\GrainPack'];
%grainPack.saveBinaryImageStack(stackFileName, 1);
%grainPackFileName = fullfile(grainDirectory, [grainDirectoryName, '_grainPack.mat']);
%save (grainPackFileName, 'grainPack')

end