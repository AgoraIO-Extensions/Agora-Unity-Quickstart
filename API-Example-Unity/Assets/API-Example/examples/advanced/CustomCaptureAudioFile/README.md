# CustomCaptureAudioFile

This demo is actually a tool that can be used as a driver to test audio.  Build this tool to a desired platform standalone and use it as the remote user.  It will play the sound file in streamingAssets/audio/myaudio continuously.  The code is based in the CustomCaptureAudio sample, where PullAudioThread pulls the audio from a Unity AudioSource.  In this sample, **myaudio** is a binary audio data clip file that is loaded in the AudioSource during start up.

## How to generate a compatible binary audio file
Subfolder Generator contains a scene **AudioFileMakerScene** that converts a AudioClip to a binary audio data file.  Simply drag and drop an compatible (.mp3, .ogg, etc) into the AudioSource and run the code.


