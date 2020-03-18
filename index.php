<!DOCTYPE html>
<html>
  <head>
    <title>Amamento Content Library</title>
    <meta name="description" content="Momento - Content">
    <script src="https://aframe.io/releases/0.7.0/aframe.min.js"></script>
    <script>

    </script>
  </head>
  <body>
    
    <a-scene stats fog="color: #241417; near: 0; far: 30;" raycaster="far: 100; objects: [link];" cursor="rayOrigin: mouse" camera-position>
      <a-assets>
        <a-asset-item id="console-obj" src="Console.obj"></a-asset-item>
        <a-asset-item id="console-mtl" src="Console.mtl"></a-asset-item>
		
        <img id="planeThumb" src="planeThumb.jpg">
        <img id="highSpeedRailThumb" src="trainThumb.jpg">
        <img id="amaOldHomeThumb" src="amaHomeThumb.jpg">
        
      </a-assets>
      <a-entity laser-controls="hand: left"></a-entity>
	  <a-entity laser-controls="hand: right"></a-entity>
      <a-entity id="console" obj-model="obj: #console-obj; mtl: #console-mtl"" position="0 0.5 2" scale="0.1 0.1 0.1" rotation="0 -180 0" ></a-entity>                                                                                                                                   
		<a-link href="plane.html" position="-3.5 1.5 -1.0" image="#planeThumb" title="Plane"></a-link>
		<a-link href="highSpeedRail.html" position="0 1.5 -1.0" image="#highSpeedRailThumb" title="Train"></a-link>
		<a-link href="amaOldHome.html" position="3.5 1.5 -1.0" image="#amaOldHomeThumb" title="Ama Old Home"></a-link>
      <a-sky color="#ECECEC"></a-sky>
    </a-scene>
  </body>
</html>