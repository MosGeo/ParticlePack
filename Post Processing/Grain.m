classdef Grain < matlab.mixin.SetGet
   properties
      Vertices
      Faces
      name
      rotation
      scale
      position
      nVertices
      normals
      XData
      YData
      ZData
   end
   
   methods 
       
      function obj = Grain(val)
        if nargin > 0
        obj.nVertices = 0;
        obj.Vertices = [];
        obj.Faces = [];
        obj.normals = [];
        obj.name = ''; 
        obj.position = [0, 0, 0];
        obj.scale = [1, 1, 1];
        obj.rotation = [0, 0, 0];
        end
      end
      
      function setMeshData(obj,Vertices, Faces, normals)
        obj.nVertices = size(Vertices,1);
        obj.Vertices = Vertices;
        obj.Faces = Faces;
        obj.normals = normals;
        obj.XData = Vertices(:,1);
        obj.YData = Vertices(:,2);
        obj.ZData = Vertices(:,3);
      end
      
      function setName(obj,name)
         obj.name = name; 
      end
      
      function setPRSdata(obj, position, rotation, scale)
        obj.position = position;
        obj.scale = rotation;
        obj.rotation = scale;  
      end
      
      function [position, rotation, scale] =  getPRSdata(obj)
        position = obj.position;
        rotation = obj.scale ;
        scale = obj.rotation ;  
      end
   
      function FV = getFV(obj)
         FV.Vertices = obj.Vertices;
         FV.Faces = obj.Faces;
      end
      
      function scaleGrain(obj, scaleVector)
        obj.Vertices = obj.Vertices .* repmat(scaleVector, obj.nVertices,1);
        obj.scale = obj.scale .* scaleVector;
      end
      
      function translateGrain(obj, translationVector)
          obj.Vertices = obj.Vertices + repmat(translationVector, obj.nVertices,1);
          obj.position = obj.position +  translationVector;
      end
      
      function rotateGrain(obj, eularAngles)
        thetaX = deg2rad(eularAngles(1));
        thetaY = deg2rad(eularAngles(2));
        thetaZ = deg2rad(eularAngles(3));

        Rx = [1 0 0; 0 cos(thetaX) -sin(thetaX); 0 sin(thetaX) cos(thetaX)];
        Ry = [cos(thetaY) 0 sin(thetaY); 0 1 0; -sin(thetaY) 0 cos(thetaY)];
        Rz = [cos(thetaZ) -sin(thetaZ) 0; sin(thetaZ) cos(thetaZ) 0; 0 0 1];
        
        roationMatrix = Rz * Ry * Rx;
        obj.Vertices = obj.Vertices * roationMatrix';
        obj.rotation = obj.roation + eularAngles;
      end
      
      function plotGrain(obj,faceColorScale, zScaleRange)

         %'FaceColor',       [0.8 0.8 1.0]
         figure('color', 'white');
         FV = obj.getFV();
         h = patch(FV,...
                'FaceColor', 'interp', ...
                 'EdgeColor',       'none',        ...
                 'FaceLighting',    'gouraud',     ...
                 'AmbientStrength', 0.15);
             
         if exist('faceColorScale', 'var')
            set(h,'CData',faceColorScale)
            colorbar
         end
         
         if exist('zScaleRange', 'var')
           caxis(zScaleRange)  
         end
        % Add a camera light, and tone down the specular highlighting
        camlight('headlight');
        material('dull');

        % Fix the axes scaling, and set a nice view angle
        axis('image');
        view([-135 35]);
        box on
        grid on;
        set(gca,'YTickLabel',[]);
        set(gca,'XTickLabel',[]);
        set(gca,'ZTickLabel',[]);
        %title(obj.name, 'Interpreter', 'none');
      end
      
      function [Cmean,Cgaussian,Dir1,Dir2,Lambda1,Lambda2] =  calculateCurvature(obj, isPlot, zScaleRange)
          if exist('isPlot', 'var') == false; isPlot = false; end
          [Cmean,Cgaussian,Dir1,Dir2,Lambda1,Lambda2]=patchcurvature(obj.getFV(),true);
          
          usedMeasurment = Cmean;
          if isPlot == true
            if exist('zScaleRange', 'var') == false
                obj.plotGrain(usedMeasurment)
            else
                obj.plotGrain(usedMeasurment, zScaleRange) 
            end
          end
      end
      
      function [sphericity, volume, surfaceArea] = calculateSphericity(obj)
        obj.Vertices
        FV.Vertices = obj.Vertices;
        FV.Faces = obj.Faces;
        [sphericity, volume, surfaceArea] = sphericityIndex(FV);
        
      end
      
   end
   
end