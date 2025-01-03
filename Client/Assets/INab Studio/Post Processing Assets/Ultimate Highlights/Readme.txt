Support: Discord (https://discord.gg/K88zmyuZFD) 
Online Documentation: https://inabstudios.gitbook.io/ultimate-outlines-and-highlights/

Import unity package from the main folder based on whether you are using URP (choose URP 2022 for 2022 LTS and URP 6 for Unity 6) or Built-in pipeline

If you have any questions feel free to contact me:
izzynab.publisher@gmail.com

### Demo Scenes

Demo scenes can be found: \Common (URP or Built-In)\Demo Scenes

Offline how-to-use:

========================
==========URP===========
========================

Step 1)
Add Highlights Manager renderer feature to your URP Renderer Data.
If you have troubles with finding your URP Asset, navigate to project settings.
You could also use one of the included in the package URP assets to test the demo scene.
Do not forget about different URP assets that are used for different quality modes.


========================
========Built-in========
========================

Step 1)
Add the Highlights Manager script to your camera.


========================
========================
========================

Step 2)
Include all 6 shaders that are used by the asset in the Always Included Shaders tab in Project Settings/Graphics:
Highlighters/AlphaBlit; Highlighters/Blit; Highlighters/SceneDepthShader; Highlighters/MeshOutlineObjects; Highlighters/Overlay; Highlighters/ObjectsInfo
Depending on the rendering pipeline shaders may vary. You can find all the shaders you need to include in the list in \Core(URP or Built-In)\Shaders

Step 3)
Open Showcases scene located at URPHighlighters\Scenes. You can find examples of different usage of the asset so you could easily understand how it works. 

There is no need to use any other scripts than Highlighter.cs, other scripts are made for you to make your life easier. 


========================
====Frequent issues=====
========================


If you do not see any highlight effects in the showcase scene after importing and setting up the asset, you may need to remove and re-add the Highlight Manager Renderer feature in your URP Renderer Data asset.

If you are experiencing errors in the console, you may want to try reimporting the asset to see if that resolves the issue.

If you opened the demo scene but are not seeing any highlights, it is possible that you are not using the Highlights Manager renderer feature in your URP Asset. Please try reviewing the setup tab again to see if this resolves the issue.

The highlights feature does not currently work with the 2D renderer in URP. It is currently not possible to inject custom render passes in this renderer.

See online documentation for further information.


### ISSUES
If you encounter any difficulties using, implementing, or understanding the asset, please feel free to contact me.