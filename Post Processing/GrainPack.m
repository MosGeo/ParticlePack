classdef GrainPack < handle
   % A class to contain all the grains of a grain pack. You can use it to
   % binarize a grain pack and do some simple visualizations and calculate
   % prosotiy
   
    properties
      nGrains
      grains
      boxDimensions
      resolution
      labeledImage
      fullLabeledImage
      grainTypes
   end
   
   methods
   
       % =================================================================
       % Constructor
       function obj = GrainPack(grainDirectory)
            [grains] = obj.loadGrains(grainDirectory);
            obj.nGrains = numel(grains);
            obj.grains = grains;
            obj.optimizeBoxDimensions(.01);
       end 
       % =================================================================
       function [grains] = loadGrains(obj, grainDirectory)
            grainsListing = dir(fullfile(grainDirectory, 'Grain*.stl'));
            nGrains =  numel(grainsListing);

            % load Results File
            nameColumn = 1;
            positionColumns = [2 3 4];
            rotationColumns = [5 6 7];
            scaleColumns = [8 9 10];
            resultsFileName = 'Results.dat';
            formatSpec = '%s%s%s%s%s%s%s%s%s%s%s%s%s%s';
            resultsFileName = fullfile(grainDirectory, resultsFileName);
            fileID = fopen(resultsFileName,'r');
            dataArray = textscan(fileID, formatSpec, 'Delimiter', ' ', 'MultipleDelimsAsOne', true, 'HeaderLines' ,1, 'ReturnOnError', false);
            fclose(fileID);
            cell2num = @(inputCell) arrayfun(@(x) str2double(x), inputCell);
            grainNames = dataArray{nameColumn};
            grainPosition = [cell2num(dataArray{positionColumns(1)}) cell2num(dataArray{positionColumns(2)}) cell2num(dataArray{positionColumns(3)})];
            grainRotation = [cell2num(dataArray{rotationColumns(1)}) cell2num(dataArray{rotationColumns(2)}) cell2num(dataArray{rotationColumns(3)})];
            grainScale = [cell2num(dataArray{scaleColumns(1)}) cell2num(dataArray{scaleColumns(2)}) cell2num(dataArray{scaleColumns(3)})];
            clear nameColumn positionColumns rotationColumns scaleColumns formatSpec fileID resultsFileName dataArray

            grains(nGrains) = Grain(nGrains);
            for grainNumber = 1:nGrains
                disp(['Loading Grain ' num2str(grainNumber) ' Out of ' num2str(nGrains)])
                selectedGrainName = grainsListing(grainNumber).name;
                grainNameExtract = strsplit(selectedGrainName, ' - ');
                grainNameExtract = grainNameExtract{2};
                [~, ind] = ismember(grainNameExtract(1:end-4), grainNames);
                grainFileName = fullfile(grainDirectory, selectedGrainName);
                [vertices,faces,normals,~] = stlRead(grainFileName);
                grains(grainNumber).setMeshData(vertices, faces, normals);
                grains(grainNumber).setName(selectedGrainName);
                grains(grainNumber).setPRSdata(grainPosition(ind,:), grainRotation(ind,:), grainScale(ind,:))
                grains(grainNumber).translateGrain(grainPosition(ind,:));
            end       
       end
       % =================================================================
       % Insert the grains into the grain pack
       function setGrains(obj, grains)
            obj.nGrains = numel(grains);
            obj.grains = grains;
       end  
       % =================================================================
       % Get the smallest box dimensions
       function optimizeBoxDimensions(obj, bufferPercentage)
          allVertices = arrayfun(@(x) x.Vertices,obj.grains, 'UniformOutput', false);
          allVertices = cell2mat(allVertices');
          minBox = min(allVertices);
          maxBox = max(allVertices);
          bufferBox = (maxBox-minBox)* bufferPercentage;
          minBox = minBox - bufferBox;
          maxBox = maxBox + bufferBox;
          obj.boxDimensions = [minBox; maxBox];
       end
       
       % ================================================================= 
       % Binarize the grains
       function [labeledImage] = createBinaryGrainPack(obj, resolution, isParallel)
           if exist('isParallel', 'var') == false
              isParallel = false; 
           end
           
           if numel(resolution) == 1
               resolution = repmat(resolution,3,1);
           end
           minBox = obj.boxDimensions(1,:);
           maxBox = obj.boxDimensions(2,:);
           
           [X,Y,Z] = meshgrid(minBox(1):resolution(1):maxBox(1), minBox(2):resolution(2):maxBox(2), minBox(3):resolution(3):maxBox(3));  %volume mesh
           imageSize = size(X);
           labeledImage = zeros(numel(X),1);
           X = X(:); Y = Y(:); Z = Z(:);
           grainMask = cell(1, obj.nGrains);
           grainImage = cell(1, obj.nGrains);
           if isParallel == false
                for grainNumber = 1:obj.nGrains
                   disp(['Analyzing Grain ' num2str(grainNumber) ' Out of ' num2str(obj.nGrains)])
                   vertices = obj.grains(grainNumber).Vertices;
                   grainMask{grainNumber} = obj.createGrainMask(vertices, X, Y, Z);
                   grainImage{grainNumber}= obj.checkPointLocation(vertices, X, Y, Z, grainMask{grainNumber});
               end
           else
               parfor grainNumber = 1:obj.nGrains
                   disp(['Analyzing Grain ' num2str(grainNumber) ' Out of ' num2str(obj.nGrains)])
                   vertices = obj.grains(grainNumber).vertices;
                   grainMask{grainNumber}= obj.createGrainMask(vertices, X, Y, Z);
                   grainImage{grainNumber}  = obj.checkPointLocation(vertices, X, Y, Z, grainMask{grainNumber});
               end
           end
           
           for grainNumber = 1:obj.nGrains
                disp(['Finalizing Grain ' num2str(grainNumber) ' Out of ' num2str(obj.nGrains)])
                currentGrainMask  =  grainMask{grainNumber};
                currentGrainImage = grainImage{grainNumber};
                labeledImage(currentGrainMask(currentGrainImage)) = grainNumber;
           end
           
        labeledImage = reshape(labeledImage,imageSize);
        
        obj.fullLabeledImage = labeledImage;
        obj.labeledImage = labeledImage;
        obj.resolution = resolution;
       end
       
       % =================================================================
       % Internal function: creats a grain mask for one grain
       function grainMask = createGrainMask(obj, vertices, X, Y, Z)
           xLocation = X>=min(vertices(:,1)) & X<=max(vertices(:,1));
           ylocation = Y>=min(vertices(:,2)) & Y<=max(vertices(:,2));
           Zlocation = Z>=min(vertices(:,3)) & Z<=max(vertices(:,3));
           grainMask = find(xLocation & ylocation & Zlocation);
       end      
       % =================================================================
       % Internal function: determine if certian points are inside the
       % grain
       function grainImage = checkPointLocation(obj, vertices, X, Y, Z, grainMask)
               tri = delaunayTriangulation(vertices);                                %triangulation
               SI = pointLocation(tri,X(grainMask),Y(grainMask),Z(grainMask));       %index of simplex (returns NaN for all points outside the convex hull)
               grainImage= ~isnan(SI); 
               %grainImage = inpolyhedron(faces,vertices,[X(grainMask),Y(grainMask),Z(grainMask)]);
               %grainImage = intriangulation(vertices,faces,[X(grainMask),Y(grainMask),Z(grainMask)]);   
               %grainImage = inhull([X(grainMask),Y(grainMask),Z(grainMask)], vertices);
       end
       % =================================================================
       % Visualize grains in 3D, not recommended
       function visualize3D(obj, image, isovalue)
           if ~exist('image', 'var'); image = obj.labeledImage; end
           if ~exist('isovalue', 'var'); isovalue = 0.9; end

           figure
           patch(isosurface(image,isovalue),'FaceColor', [0.8 0.8 1.0], ...
                     'EdgeColor','none',...
                     'FaceLighting','gouraud',...
                     'AmbientStrength', 0.15);
           camlight('headlight');
           view(-78,22)
           material('dull');
           axis equal;
       end
       
       % =================================================================
       % Visualize slices
       function ax = visualizeSlices(obj, xSlicesPercentage, ySlicesPercentage, zSlicesPercentage, isBinary)
           if ~exist('isBinary', 'var'); isBinary = true; end
           
           if isBinary
                image = obj.getBinaryImage(); 
           else
                image = obj.labeledImage;
           end
           
           imageSize = size(image);
           sx = round(imageSize(1) * xSlicesPercentage);
           sy = round(imageSize(2) * ySlicesPercentage);
           sz = round(imageSize(3) * zSlicesPercentage);
           sx(sx==0) = 1;
           sy(sy==0) = 1;
           sz(sz==0) = 1;
           
           figure('Color', 'White')
           h = slice(image,sx,sy,sz);
           minValue = inf;
           maxValue = -inf;
           for i = 1:numel(h)
                h(i).EdgeColor = 'none';
                minValue = min([h(1).CData(:); minValue]);
                maxValue = max([h(1).CData(:); maxValue]);
           end
           caxis([minValue, maxValue]);
           colormap gray
           axis equal;
           box on
           ax = gca;
       end   
       % =================================================================
       % Extract a sub-cube rom the image
       function subImage =  extractSubVolume(obj, xBuffer, yBuffer,zBuffer, image)
           if ~exist('image', 'var'); image = obj.fullLabeledImage; end
           
           imageSize = size(image);  
           sx = round(imageSize(1) * xBuffer);
           sy = round(imageSize(2) * yBuffer);
           sz = round(imageSize(3) * zBuffer);
           
           if (numel(sx) == 1); sx=[sx sx]; end
           if (numel(sy) == 1); sy=[sy sy]; end
           if (numel(sz) == 1); sz=[sz sz]; end
           subImage = image(1+sx(1):end-sx(2), 1+sy(1):end-sy(2), 1+sz(1):end-sz(2));
                     
           obj.labeledImage = subImage;
       end     
       % =================================================================
       % Reset sub-cube extraction
       function resetSubVolume(obj)
        obj.labeledImage = obj.fullLabeledImage;
       end
       
       % =================================================================
       % Get porosity
       function porosity = calculatePorosity(obj, image)
             if exist('image', 'var') == false
               image = obj.labeledImage;
             end
             
             if iscell(image) == true
                image = image{1}; 
             end
             
             porosity = 1 - sum(image(:))/numel(image);
       end    
       % =================================================================
       % Save image in on tif
       function saveImage(obj,outputFileName, image)
           if ~exist('outputFileName', 'var'); outputFileName = 'Image.tif'; end
           if ~exist('image', 'var'); image = obj.labeledImage; end
           
            for K=1:length(image(1, 1, :))
               imwrite(image(:, :, K), outputFileName, 'WriteMode', 'append',  'Compression','none');
            end 
       end
       % =================================================================
       % Save image as slices
       function saveImageStack(obj,outputFileName, direction, nDigits, extension, image)
         
           % Default Values
           if exist('outputFileName', 'var') == false; outputFileName = 'image';end
           if exist('image', 'var') == false; image = obj.labeledImage; end
           if exist('direction', 'var') == false; direction = 3;end
           if exist('nDigits', 'var') == false; nDigits = 0;end
           if exist('extension', 'var') == false; extension = 'tif';end

           
           for K=1:size(image,direction)
               
               Kstring = num2str(K);
               if length(Kstring)<nDigits
                   KstringExtra = repmat('0',1, nDigits-length(Kstring));
                   Kstring = [KstringExtra, Kstring];
               end
               
               switch direction 
               case 3
                   im = imrotate(squeeze(image(:, :, K)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);
               case 2
                   im = imrotate(squeeze(image(:, K, :)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);
               case 1
                   im = imrotate(squeeze(image(K, :, :)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);  
               end
           end    
      end
      % =================================================================
      % Saves the grain pack
      function saveGrainPack(obj, outputFileName)
            save(outputFileName, 'obj');
      end
      % =================================================================
      function binaryImage = getBinaryImage(obj)
          binaryImage = double(obj.labeledImage>0);
      end
      % =================================================================
      % Calculate subsample porosity
       function [porosities, dDimension, nSplits] = calculateSubsamplePorosity(obj, devision, isSplitOrWidth, binaryImage)
         
        if ~exist('binaryImage', 'var'); binaryImage = obj.getBinaryImage; end
        if ~exist('devision', 'var'); devision = 10; end
        if ~exist('isSplitOrWidth', 'var'); isSplitOrWidth = 'split'; end
         
        switch isSplitOrWidth
            case 'split'
            [subSamples, dDimension, nSplits] = obj.subSampleByNumberOfSplits(devision, binaryImage);
            case 'width'
            [subSamples, dDimension, nSplits] = obj.subSampleByWidth(devision, binaryImage);
        end
        
        functionToApply = @(x) obj.calculatePorosity(x);
        porosities = arrayfun(functionToApply, subSamples);
     end
     
     % =================================================================     
     function [subSamples, dDimension, nSplits] = subSampleByNumberOfSplits(obj, nSplits, image)
        if ~exist('image', 'var'); image = obj.labeledImage; end 
        if ~exist('nSplits', 'var'); nSplits = 10; end
        
        imSize = size(image);
        if (numel(nSplits) == 1)
            nSplits = [nSplits, nSplits, nSplits]; 
        end

        dDimension = floor(imSize./nSplits);
        imCropped = image(1:nSplits(1)*dDimension(1), 1:nSplits(2)*dDimension(2), 1:nSplits(3)*dDimension(3));

        Xs = repmat(dDimension(1), nSplits(1),1);
        Ys = repmat(dDimension(2), nSplits(2),1);
        Zs = repmat(dDimension(3), nSplits(3),1);
        subSamples = mat2cell(imCropped, Xs, Ys, Zs);
        subSamples = subSamples(:);
         
     end
     % =================================================================    
     function [subSamples, dDimension, nSplits] = subSampleByWidth(obj, dDimension, image)
        if ~exist('image', 'var'); image = obj.labeledImage; end       
        if ~exist('dDimension', 'var'); dDimension = 50; end
        
        imSize = size(image);
        if (numel(dDimension) == 1)
            dDimension = [dDimension, dDimension, dDimension]; 
        end
         
        nSplits = floor(imSize./dDimension);
        imCropped = image(1:nSplits(1)*dDimension(1), 1:nSplits(2)*dDimension(2), 1:nSplits(3)*dDimension(3));  
        
        Xs = repmat(dDimension(1), nSplits(1),1);
        Ys = repmat(dDimension(2), nSplits(2),1);
        Zs = repmat(dDimension(3), nSplits(3),1);
        subSamples = mat2cell(imCropped, Xs, Ys, Zs);
        subSamples = subSamples(:);
         
     end
     % =================================================================
     % Calculate sphericity
     function [sphericity, volume, surfaceArea] = calculateSphericity(obj, isPlot)
        if ~exist('isPlot', 'var'); isPlot = false; end
        
        sphericity = zeros(obj.nGrains,1);
        
        volume = zeros(obj.nGrains,1);
        surfaceArea = zeros(obj.nGrains,1);

        for i=1:obj.nGrains
            [sphericity(i), volume(i), surfaceArea(i)] = obj.grains(i).calculateSphericity();
        end
        
        if isPlot == true
           histogram(sphericity); 
        end
        
     end
     % =================================================================
     % Calculate curvatures
     function [Cmean,Cgaussian] =  calculateMeanGrainCurvature(obj)
        if ~exist('isPlot', 'var'); isPlot = false; end
        
        Cmean= zeros(obj.nGrains,1);
        Cgaussian= zeros(obj.nGrains,1);
        Dir1= zeros(obj.nGrains,1);
        Dir2= zeros(obj.nGrains,1);
        Lambda1= zeros(obj.nGrains,1);
        Lambda2= zeros(obj.nGrains,1);
        
        for i=1:obj.nGrains
            [CmeanG,CgaussianG] =  obj.grains(i).calculateCurvature();
            Cmean(i) = mean(CmeanG);
            Cgaussian(i) = mean(CgaussianG);
        end
                 
         if isPlot == true
           histogram(Cmean); 
        end
     end
     % =================================================================
     function [typedImage, typeInfo, nTypes] = getTypedImage(obj)
         
         names = {obj.grains(:).name};
         grainInfo = cellfun(@(x) sscanf(x,'Grain %i - B%i-G%i-I%i'), names, 'UniformOutput', false);
         grainInfo = cell2mat(grainInfo)';
         BGinfo = grainInfo(:,2:3);
         %BGinfo(50:end,2)=2;
         [typeInfo,~,typeLabel] = unique(BGinfo, 'rows');
         grainNumber = (1:obj.nGrains)';
         nTypes = size(typeInfo,1);
         
         typedImage = obj.labeledImage;
         for currentType = 1:nTypes
             currentGrainNumbers = grainNumber(typeLabel==currentType);
             currentVoxels = ismember(obj.labeledImage, currentGrainNumbers);
             typedImage(currentVoxels) = currentType;
         end    
     end
     % =================================================================
     function [labeledImage] = getLabeledImage(obj)      
        labeledImage = obj.labeledImage;  
     end
     % =================================================================
     
   end
   
   
end