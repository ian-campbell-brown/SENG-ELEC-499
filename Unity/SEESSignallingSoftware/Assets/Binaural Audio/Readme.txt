=========
= About 
=========

Package: Binaural Audio
Version: v1.0

By: Jason Lim


This script has been developed to be used with the Head Related Impulse Response Matlab files publicly provided by the University of Calgary CIPIC Interface Laboratory HRTF database. Using this script allows the creation of live Binaural 3D audio in Unity.

=========
= Usage 
=========

Before using this script, you must copy two files from the Plugins/Native folder into the root of your Unity project directory. Your project also must allow unsafe code If it does not already do so, copy or merge the files from Config into the root of your Assets folder.

To use the script, simply add the Binaural Audio Filter to an object with an AudioSource attached to it. The associated AudioClip must be a 3D sound in order for the script to run. To load an HRIR file, simply import the Matlab file into Unity and it will produce an HRIR Asset file which you can then attach to the script.

This script requires the -unsafe C-sharp Editor Script compiler option

==============
= HRIR Files 
==============

HRIR files can be downloaded from the following webpage:

http://interface.cipic.ucdavis.edu/sound/hrtf.html


=========
= Notes 
=========

For the best results, it is suggested that all regular 3D audio effects be disabled. However, if you do decide to use 3D audio effects, be sure to change the Sample Source to TwoChannels or else the audio may sound unbalanced between ears.


As this script is implemented in software, running multiple binaural audio sources at once may result in some of the audio skipping.


The HRIR importer will not work for the Special Kemar HRIR files as they contain a different set of measurements than the standard HRIR database entries.
