classdef GrainPack < handle
   properties
      nGrains
      grains
      boxDimensions
      binaryResolution
      binaryImage
      fullBinaryImage
      %labeledImage
      %fullLabeledImage
   end
   
   methods
   
       function obj = GrainPack(grains)
            obj.nGrains = numel(grains);
            obj.grains = grains;
            obj.optimizeBoxDimensions(.01);
       end
       
       function setGrains(obj, grains)
            obj.nGrains = numel(grains);
            obj.grains = grains;
       end
       
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
       
       function binaryImage = createBinaryGrainPack(obj, resolution, isParallel)
           if exist('isParallel', 'var') == false
              isParallel = false; 
           end
           
           if numel(resolution) == 1
               resolution = repmat(resolution,3,1);
           end
           minBox = obj.boxDimensions(1,:);
           maxBox = obj.boxDimensions(2,:);
           
           [X,Y,Z] = meshgrid(minBox(1):resolution(1):maxBox(1), minBox(2):resolution(2):maxBox(2), minBox(3):resolution(3):maxBox(3));  %volume mesh
           binaryImageSize = size(X);
           binaryImage = zeros(numel(X),1);
           %labeledImage = zeros(numel(X),1);
           X = X(:); Y = Y(:); Z = Z(:);
           grainMask = cell(1, obj.nGrains);
           grainImage = cell(2, obj.nGrains);
           
           if isParallel == false
                for grainNumber = 1:obj.nGrains
                   disp(['Analyzing Grain ' num2str(grainNumber) ' Out of ' num2str(obj.nGrains)])
                   vertices = obj.grains(grainNumber).Vertices;
                   grainMask{grainNumber}= obj.createGrainMask(vertices, X, Y, Z);
                   grainImage{grainNumber}  = obj.checkPointLocation(vertices, X, Y, Z, grainMask{grainNumber});
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
            currentGrainMask =  grainMask{grainNumber};   
            binaryImage(currentGrainMask) = binaryImage(currentGrainMask) | grainImage{grainNumber};
            
            %updatedLabel = labeledImage(currentGrainMask) + grainImage{grainNumber}*grainNumber;
            %updatedLabel(binaryImage(currentGrainMask) & grainImage{grainNumber}) = grainNumber;
            %labeledImage(currentGrainMask) =  updatedLabel;
           end
           
        binaryImage = reshape(binaryImage,binaryImageSize);                 %reshape the mask 
        %labeledImage = reshape(labeledImage,binaryImageSize);
        
        obj.binaryImage= binaryImage;
        obj.fullBinaryImage = binaryImage;
        %obj.fullLabeledImage = labeledImage;
        %obj.labeledImage = labeledImage;
        obj.binaryResolution = resolution;
       end
       
       function grainMask = createGrainMask(obj, vertices, X, Y, Z)
           xLocation = X>=min(vertices(:,1)) & X<=max(vertices(:,1));
           ylocation = Y>=min(vertices(:,2)) & Y<=max(vertices(:,2));
           Zlocation = Z>=min(vertices(:,3)) & Z<=max(vertices(:,3));
           grainMask = find(xLocation & ylocation & Zlocation);
       end
       
       function grainImage = checkPointLocation(obj, vertices, X, Y, Z, grainMask)
               tri = delaunayTriangulation(vertices);                                %triangulation
               SI = pointLocation(tri,X(grainMask),Y(grainMask),Z(grainMask));       %index of simplex (returns NaN for all points outside the convex hull)
               grainImage= ~isnan(SI); 
               %grainImage = inpolyhedron(faces,vertices,[X(grainMask),Y(grainMask),Z(grainMask)]);
               %grainImage = intriangulation(vertices,faces,[X(grainMask),Y(grainMask),Z(grainMask)]);   
               %grainImage = inhull([X(grainMask),Y(grainMask),Z(grainMask)], vertices);
       end
       
       function visulize3D(obj, binaryImage)
           if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
           end
           figure
           patch(isosurface(binaryImage,.9),'FaceColor', [0.8 0.8 1.0], ...
                     'EdgeColor','none',...
                     'FaceLighting','gouraud',...
                     'AmbientStrength', 0.15);
           camlight('headlight');
           view(-78,22)
           material('dull');
           axis equal;
       end
       
       function visulizeSlices(obj, xSlicesPercentage, ySlicesPercentage, zSlicesPercentage, binaryImage)
           if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
           end
           
           binaryImageSize = size(binaryImage);
           sx = round(binaryImageSize(1) * xSlicesPercentage);
           sy = round(binaryImageSize(2) * ySlicesPercentage);
           sz = round(binaryImageSize(3) * zSlicesPercentage);
           sx(sx==0) = 1;
           sy(sy==0) = 1;
           sz(sz==0) = 1;
           h = slice(binaryImage,sx,sy,sz);
           for i = 1:numel(h)
            %h(i).FaceColor = 'interp';
            h(i).EdgeColor = 'none';
           end
           colormap gray
           axis equal;
           box on
       end
       
       function binarySubImage =  extractSubVolume(obj, xBuffer, yBuffer,zBuffer, binaryImage)
           if exist('binaryImage', 'var') == false
               binaryImage = obj.fullBinaryImage;
               %labeledImage = obj.fullLabeledImage;
           end
           binaryImageSize = size(binaryImage);
           
           sx = round(binaryImageSize(1) * xBuffer);
           sy = round(binaryImageSize(2) * yBuffer);
           sz = round(binaryImageSize(3) * zBuffer);
           
           if (numel(sx) == 1); sx=[sx sx]; end
           if (numel(sy) == 1); sy=[sy sy]; end
           if (numel(sz) == 1); sz=[sz sz]; end
           binarySubImage=binaryImage(1+sx(1):end-sx(2), 1+sy(1):end-sy(2), 1+sz(1):end-sz(2));
           %labeledSubImage=labeledImage(1+sx(1):end-sx(2), 1+sy(1):end-sy(2), 1+sz(1):end-sz(2));

           obj.binaryImage = binarySubImage;
           %obj.labeledImage= labeledSubImage;
       end
       
       function resetSubVolume(obj)
        obj.binaryImage= obj.fullBinaryImage;
       end
       
       function porosity = calculatePorosity(obj, binaryImage)
             if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
             end
             
             if iscell(binaryImage) == true
                binaryImage = binaryImage{1}; 
             end
             
             porosity = 1 - sum(binaryImage(:))/numel(binaryImage);
       end
       
       function saveBinaryImage(obj,outputFileName, binaryImage)
           if exist('outputFileName', 'var') == false
               outputFileName = 'binaryImage.tif';
           end
           if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
           end
           
            for K=1:length(binaryImage(1, 1, :))
               imwrite(binaryImage(:, :, K), outputFileName, 'WriteMode', 'append',  'Compression','none');
            end 
       end
       
     function saveBinaryImageStack(obj,outputFileName, direction, nDigits, extension, binaryImage)
         
           % Default Values
           if exist('outputFileName', 'var') == false; outputFileName = 'binaryImage';end
           if exist('binaryImage', 'var') == false; binaryImage = obj.binaryImage; end
           if exist('direction', 'var') == false; direction = 3;end
           if exist('nDigits', 'var') == false; nDigits = 0;end
           if exist('extension', 'var') == false; extension = 'tif';end

           
           for K=1:size(binaryImage,direction)
               
               Kstring = num2str(K);
               if length(Kstring)<nDigits
                   KstringExtra = repmat('0',1, nDigits-length(Kstring));
                   Kstring = [KstringExtra, Kstring];
               end
               
               switch direction 
               case 3
                   im = imrotate(squeeze(binaryImage(:, :, K)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);
               case 2
                   im = imrotate(squeeze(binaryImage(:, K, :)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);
               case 1
                   im = imrotate(squeeze(binaryImage(K, :, :)), 90);
                   imwrite(im, [outputFileName '_' Kstring '.' extension]);  
               end
           end    
     end
       
     function saveGrainPack(obj, outputFileName)
        save(outputFileName, 'obj');
     end
     
     function [porosities, dDimension, nSplits] = calculateSubsamplePorosity(obj, devision, isSplitOrWidth, binaryImage)
         
        if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
        end
        
         if exist('devision', 'var') == false
               devision = 10;
         end
        
         if exist('isSplitOrWidth', 'var') == false
            isSplitOrWidth = 'split'; 
         end
         
        switch isSplitOrWidth
            case 'split'
            [subSamples, dDimension, nSplits] = obj.subSampleByNumberOfSplits(devision, binaryImage);
            case 'width'
            [subSamples, dDimension, nSplits] = obj.subSampleByWidth(devision, binaryImage);
        end
        
        functionToApply = @(x) obj.calculatePorosity(x);
        porosities = arrayfun(functionToApply, subSamples);
     end
     
     function [subSamples, dDimension, nSplits] = subSampleByNumberOfSplits(obj, nSplits, binaryImage)
        if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
        end
        
        if exist('nSplits', 'var') == false
               nSplits = 10;
        end
        
        imSize = size(binaryImage);

        if (numel(nSplits) == 1)
            nSplits = [nSplits, nSplits, nSplits]; 
        end

        dDimension = floor(imSize./nSplits);
        imCropped = binaryImage(1:nSplits(1)*dDimension(1), 1:nSplits(2)*dDimension(2), 1:nSplits(3)*dDimension(3));

        Xs = repmat(dDimension(1), nSplits(1),1);
        Ys = repmat(dDimension(2), nSplits(2),1);
        Zs = repmat(dDimension(3), nSplits(3),1);
        subSamples = mat2cell(imCropped, Xs, Ys, Zs);
        subSamples = subSamples(:);
         
     end
     
     function [subSamples, dDimension, nSplits] = subSampleByWidth(obj, dDimension, binaryImage)
        if exist('binaryImage', 'var') == false
               binaryImage = obj.binaryImage;
        end
        
        if exist('width', 'var') == false
               dDimension = 50;
        end
        
        imSize = size(binaryImage);

        if (numel(dDimension) == 1)
            dDimension = [dDimension, dDimension, dDimension]; 
        end
         
        nSplits = floor(imSize./dDimension);
        imCropped = binaryImage(1:nSplits(1)*dDimension(1), 1:nSplits(2)*dDimension(2), 1:nSplits(3)*dDimension(3));
        
        
        Xs = repmat(dDimension(1), nSplits(1),1);
        Ys = repmat(dDimension(2), nSplits(2),1);
        Zs = repmat(dDimension(3), nSplits(3),1);
        subSamples = mat2cell(imCropped, Xs, Ys, Zs);
        subSamples = subSamples(:);
         
     end
     
     function [sphericity, volume, surfaceArea] = calculateSphericity(obj, isPlot)
        if exist('isPlot', 'var') == false; isPlot = false; end
        
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
     
     function [Cmean,Cgaussian] =  calculateMeanGrainCurvature(obj)
        if exist('isPlot', 'var') == false; isPlot = false; end
        
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
      
   end
   
   
end