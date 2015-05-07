using UnityEngine;
using System.Collections;

using FaceExpression = PXCMFaceData.ExpressionsData.FaceExpression;
using FaceExpressionResult = PXCMFaceData.ExpressionsData.FaceExpressionResult;

public class FaceInput : MonoBehaviour {
	public delegate void OnFaceDataDelegate(PXCMFaceData.PoseEulerAngles headAngle, float mouthOpen);
	public delegate void OnColorImageDelegate(PXCMImage data);

	public event OnFaceDataDelegate OnFaceData;
	public event OnColorImageDelegate OnColorImage;

	public GameScript gameController;

	private PXCMSenseManager sm=null;
	bool ready = false;

	void Start () {
		sm = PXCMSenseManager.CreateInstance();
		if (sm == null) return;
		
		
		pxcmStatus sts = sm.EnableFace();
		PXCMFaceModule face = sm.QueryFace();
		if (face != null){
			PXCMFaceConfiguration cfg = face.CreateActiveConfiguration();
			
			if (cfg != null){
				cfg.landmarks.isEnabled = true;
				cfg.pose.isEnabled = true;
				//ecfg.EnableExpression(FaceExpression.EXPRESSION_MOUTH_OPEN);
				
				cfg.ApplyChanges();
				cfg.Dispose();
			}
		}
		
		/* Initialize the execution pipeline */ 
		sts = sm.Init(); 
		if (sts<pxcmStatus.PXCM_STATUS_NO_ERROR) {
			OnDisable();
			return;
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (sm==null) return;

		if (sm.AcquireFrame(false,0)<pxcmStatus.PXCM_STATUS_NO_ERROR) return;

		PXCMFaceModule face = sm.QueryFace();
        if (face != null){
            PXCMFaceData data = face.CreateOutput();
            if (data != null){
                data.Update();
                PXCMFaceData.Face face_data = data.QueryFaceByIndex(0);
                if (face_data != null){
					PXCMFaceData.PoseData pose = face_data.QueryPose();


					PXCMFaceData.PoseEulerAngles headAngles = null;
					if (pose != null){
						pose.QueryPoseAngles(out headAngles);
					}
					float mouthOpen = 0;
					/*PXCMFaceData.ExpressionsData expression = face_data.QueryExpressions();
					 *  FaceExpressionResult mouthOpenResult = null;
					if (expression != null){
						bool sts1 = expression.QueryExpression(FaceExpression.EXPRESSION_MOUTH_OPEN, out mouthOpenResult);
						if (sts1) mouthOpen = mouthOpenResult.intensity;
					}*/


					PXCMFaceData.LandmarksData landmarks = face_data.QueryLandmarks();
					if (landmarks != null){
						int topId = landmarks.QueryPointIndex(PXCMFaceData.LandmarkType.LANDMARK_UPPER_LIP_CENTER);
						int botId = landmarks.QueryPointIndex(PXCMFaceData.LandmarkType.LANDMARK_LOWER_LIP_CENTER);

						PXCMFaceData.LandmarkPoint top, bot;
						
						bool sts = landmarks.QueryPoint(topId, out top);
						sts = sts && landmarks.QueryPoint(botId, out bot);

						if (sts){
							mouthOpen = Mathf.Abs(top.image.y - bot.image.y);
						}
					}

					OnFaceData(headAngles, mouthOpen);

					/* Retrieve the color and depth images if ready */
					PXCMCapture.Sample sample = sm.QueryFaceSample();

					
					if (sample != null && sample.color != null)	{
						OnColorImage(sample.color);
						if (!ready){
							ready = true; gameController.GameStart();
						}
					}
                }
                data.Dispose();
            }
        }


		
		/* Now, process the next frame */
		sm.ReleaseFrame();
	}

	void OnDisable() {
		if (sm==null) return;
		sm.Dispose();
		sm=null;
	}
}
