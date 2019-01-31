
Particula - Particle Pack Generator
=====

<div align="center">
    <img width=800 src="https://github.com/MosGeo/ParticlePack/blob/master/ReadmeFiles/TopImage.png" alt="TopImage" title="Image of particle pack"</img>
</div>

The software simulates the formation of three-dimensional packings of spherical and non-spherical (regular and irregular) particles with prescribed size distributions. An efficient approach to create multiple realizations of non-spherical irregularly shaped particles using coherent noise modification of the spherical particle surface is introduced. The simulator generates loose and dense granular packings by shaking. Complex depositional styles (fining upward, coarsening upward, random) are possible. Furthermore, multiple beds can be simulated to produce geologically realistic grading.

<div align="center">
    <img width=800 src="https://github.com/MosGeo/ParticlePack/blob/master/ReadmeFiles/Grading.png" alt="Grading" title="Image of grading particle pack"</img>
</div>

## How does it work
Discrete element method is used to pour generated particles into a container. Shaking can be applied to produce denser packs.

<div align="center">
    <img width=1000 src="https://github.com/MosGeo/ParticlePack/blob/master/ReadmeFiles/Process.png" alt="Process" title="Particle Generation Process"</img>
</div>

## Requirements

The compiled software was tested on multiple Windows 10 machines. To compile the source code, [Unity3D](https://unity3d.com/) is required. Version 2018.3.3 is currently recommended. Hardware requirements depend on the desired number of particles in the packings generated. A typical laptop specification would be suitable for 5000-10000 particles. 

## Getting started

The compiled version uses a parameter file to define the simulation parameters. It can run on a Windows machine. The source code can be ran inside Unity and the same parameters can be modified in the platform. The source code can be compiled on Windows, Mac, or Linux. Please check the [Wiki Page](https://github.com/MosGeo/ParticlePack/wiki) for detailed information.

## Post processing
To create a binary image from the output, Matlab code is provided in the "Post Processing" folder. The mesh folder is also saved as an output so it can be directly used as an input for FEM solvers or be processed for 3D printing.

<div align="center">
    <img width=500 src="https://github.com/MosGeo/ParticlePack/blob/master/ReadmeFiles/BinaryImage.png" alt="BinaryImage" title="Binary image of a particle pack"</img>
</div>

## Are you using this package? Want to provide feedback?
If you are using this package. I would love to hear from you on how you use and what sort of modification that you would like to be seen. You can either create an Issue on Github or send me an email directly at Mustafa.Geoscientist@outlook.com

## Referencing
Al Ibrahim, M. A., Kerimov, A., Mukerji, T., and Mavko, G., 2018, Digital rocks with irregularly shaped grains: A simulator tool for computational rock physics: SEG Technical Program Expanded Abstracts 2018. [Link](https://agupubs.onlinelibrary.wiley.com/doi/full/10.1029/2018JB016031)

## Published research studies using Particula

Kerimov, A., Mavko, G., Mukerji, T., Dvorkin, J., and Al Ibrahim, M. A., 2018, The Influence of Convex Particles' Irregular Shape and Varying Size on Porosity, Permeability, and Elastic Bulk Modulus of Granular Porous Media: Insights From Numerical Simulations: JGR Solid Earth, v. 123, no. 12, p. 10,563-10,582. [Link](https://library.seg.org/doi/abs/10.1190/segam2018-2993373.1)

Kerimov, A., Mavko, G., Mukerji, T., and Al Ibrahim, M. A., 2018, Mechanical trapping of particles in granular media: Physical Review E, v. 97, no. 2, 19 p. [Link](https://journals.aps.org/pre/abstract/10.1103/PhysRevE.97.022907)

## Acknowledgments

We would like to thank the Stanford Rock Physics & Borehole Geophysics Project, Stanford Center for Reservoir Forecasting, and Saudi Aramco for their support and valuables discussions. We would also like to thank the Dean of School of Earth, Energy, and Environmental Sciences at Stanford University, Prof. Steve Graham, for funding.

<div align="center">
    <img width=1000 src="https://github.com/MosGeo/ParticlePack/blob/master/ReadmeFiles/PolyExample2.png" alt="Example of Grain Pack" title="Polydesperse Realistic Example"</img>
</div>

