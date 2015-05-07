using UnityEngine;
using System.Collections;

public class ImageScript : MonoBehaviour {
	public FaceInput input;
	private PXCMSizeI32 imgSize;
	private Texture2D colorImg;

	// Use this for initialization
	void Start () {
		imgSize.width = imgSize.height = 0;
		input.OnColorImage += OnColorImage;
	}

	void OnColorImage(PXCMImage image) {
		/* Save depth size for later use */
		imgSize.width=image.info.width;
		imgSize.height=image.info.height;

		

		if (colorImg==null) {
			/* If not allocated, allocate Texture2D */
			imgSize.width=image.info.width;
			imgSize.height=image.info.height;
			colorImg=new Texture2D((int)imgSize.width, (int)imgSize.height, TextureFormat.ARGB32, false);
			
			/* Associate the Texture2D with the cube */
			GetComponent<Renderer>().material.mainTexture=colorImg;
			GetComponent<Renderer>().material.mainTextureScale=new Vector2(1,-1);
		}
		
		/* Retrieve the image data in Texture2D */
		PXCMImage.ImageData data;
		image.AcquireAccess(PXCMImage.Access.ACCESS_READ,PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32,out data);
		data.ToTexture2D(0, colorImg);

		image.ReleaseAccess(data);
		/* Display on the Cube */
		/*for (int x = 0; x < rect.w; ++x) {
			colorImg.SetPixel(x+rect.x, rect.y, Color.red);
			colorImg.SetPixel(x+rect.x, rect.y + rect.h - 1, Color.red);
		}
		
		for (int y = 0; y < rect.h; ++y) {
			colorImg.SetPixel(rect.x, y + rect.y, Color.red);
			colorImg.SetPixel(rect.x + rect.w - 1, y + rect.y, Color.red);
		}*/
		
		colorImg.Apply();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
