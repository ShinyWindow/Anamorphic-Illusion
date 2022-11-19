# Anamorphic-Illusion
Right now the project is set up for a virtual environment, but to set it up to work on an actual tv is rather simple.
Steps:
  1. Implement your method of head tracking of choice.
    -In my case, I used an original Oculus Quest and UnityXR
  2. Remove the render texture from the Camera in the GameObject 'Projector'
  3. Set Projector to Main Camera
  4. Delete Other Cameras
  5. Set your head tracked GameObject as the Eye Position in the Projector GameObject
  6. Manipulate the Screen GameObject so that it has the same dimensions as the screen you're going to render to in real life.
    -1 unit in unity is equal to 1 meter (except for Planes, 1 unit for planes is 10 meters)
  6. on runtime, move virtual screen to the same position as irl screen relative to your head
  7. If you set everything up correctly, it should work exactly as it does in my video :D

You'll probably notice that it doesn't look super real. This is because your depth perception is telling your brain that you're looking at something flat. Try closing one eye!

I'm working on getting this to work with my old 3D TV, so that'll be fun.
Have fun!
