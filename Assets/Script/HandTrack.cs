/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;

public class HandTrack : MonoBehaviour
{
	public GameObject JointPrefab; //Prefab for Joints
	public GameObject TipPrefab; //Prefab for Finger Tips
	public GameObject BonePrefab; //Prafab for Bones
	public GameObject PalmCenterPrefab;//Prefab for Palm Center

	public GameObject[] puppetSpotlight;
	public GameObject puppetHandPrefab;
	public GameObject puppetSpotlightPrefab;
	private GameObject[] puppetHands;
	private PlayerScript[] puppetBehavior;

	private GameObject[][] myJoints; //Array of Joint GameObjects
	private GameObject[][] myBones; //Array of Bone GameObjects

	public GUIText txt;
	
	private PXCMHandData.JointData[][] jointData; //non-smooth joint values
	private PXCMDataSmoothing ds=null; //Smoothing module instance
	private PXCMDataSmoothing.Smoother3D[][] smoother3D= null; //smooth joint values
	private int weightsNum = 4; //smoothing factor
	
	private PXCMSenseManager sm = null; //SenseManager Instance
	private pxcmStatus sts; //StatusType Instance
	private PXCMHandModule handAnalyzer; //Hand Module Instance
	private int MaxHands = 1; //Max Hands
	private int MaxJoints = PXCMHandData.NUMBER_OF_JOINTS; //Max Joints
	
	private Hashtable handList;//keep track of bodyside and hands for GUItext

	// Use this for initialization
	void Start ()
	{
		handList = new Hashtable ();

		/* Initialize a PXCMSenseManager instance */
		sm = PXCMSenseManager.CreateInstance ();
		if (sm == null)
			Debug.LogError ("SenseManager Initialization Failed");

		/* Enable hand tracking and retrieve an hand module instance to configure */
		sts = sm.EnableHand ();
		handAnalyzer = sm.QueryHand ();
		if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
			Debug.LogError ("PXCSenseManager.EnableHand: " + sts);

		/* Initialize the execution pipeline */
		sts = sm.Init ();
		if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
			Debug.LogError ("PXCSenseManager.Init: " + sts);
		
		/* Retrieve the the DataSmoothing instance */
		sm.QuerySession ().CreateImpl<PXCMDataSmoothing> (out ds);
		
		/* Create a 3D Weighted algorithm */
		smoother3D = new PXCMDataSmoothing.Smoother3D[MaxHands][];
		
		/* Configure a hand - Enable Gestures and Alerts */
		PXCMHandConfiguration hcfg = handAnalyzer.CreateActiveConfiguration ();
		hcfg.EnableAllGestures ();
		hcfg.EnableAlert (PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED);
		hcfg.ApplyChanges ();
		hcfg.Dispose ();
		
		InitializeGameobjects ();

	}
	
	// Update is called once per frame
	void Update ()
	{
		
		/* Make sure SenseManager Instance is valid */
		if (sm == null)
			return;

		/* Wait until any frame data is available */
		if (sm.AcquireFrame (false) != pxcmStatus.PXCM_STATUS_NO_ERROR)
			return;

		/* Retrieve hand tracking Module Instance */
		handAnalyzer = sm.QueryHand ();

		if (handAnalyzer != null) {
			/* Retrieve hand tracking Data */
			PXCMHandData _handData = handAnalyzer.CreateOutput ();
			if (_handData != null) {
				_handData.Update ();
				
				/* Retrieve Gesture Data to manipulate GUIText */
				PXCMHandData.GestureData gestureData;
				for (int i = 0; i < _handData.QueryFiredGesturesNumber(); i++)
					if (_handData.QueryFiredGestureData (i, out gestureData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
						DisplayGestures (puppetBehavior[0], gestureData);
				
				
				/* Retrieve Alert Data to manipulate GUIText */
				PXCMHandData.AlertData alertData;
				for (int i=0; i<_handData.QueryFiredAlertsNumber(); i++)
					if (_handData.QueryFiredAlertData (i, out alertData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
						ProcessAlerts (alertData);
		
				/* Retrieve all joint Data */
				for (int i = 0; i < _handData.QueryNumberOfHands () && i < MaxHands; i++) {
					PXCMHandData.IHand _iHand;
					if (_handData.QueryHandData (PXCMHandData.AccessOrderType.ACCESS_ORDER_FIXED, i, out _iHand) == pxcmStatus.PXCM_STATUS_NO_ERROR) {
						for (int j = 0; j < MaxJoints; j++) {
							if (_iHand.QueryTrackedJoint ((PXCMHandData.JointType)j, out jointData [i] [j]) != pxcmStatus.PXCM_STATUS_NO_ERROR)					
								jointData [i] [j] = null;
						}
						if (!handList.ContainsKey (_iHand.QueryUniqueId ()))
							handList.Add (_iHand.QueryUniqueId (), _iHand.QueryBodySide ());
					}
				}
				
				/* Smoothen and Display the Data - Joints and Bones*/
				DisplayJoints ();

			}
			_handData.Dispose ();
		}

		handAnalyzer.Dispose ();

		sm.ReleaseFrame ();

		RotateCam ();

	}
	
	//Close any ongoing Session
	void OnDisable ()
	{
		if (smoother3D != null) {
			for (int i=0;i<MaxHands;i++) {
				if (smoother3D[i] !=null) {
					for (int j=0;j<MaxJoints;j++) {
						smoother3D[i][j].Dispose();
						smoother3D[i][j]=null;
					}
				}
			}
			smoother3D=null;
		}
		
		if (ds != null) {
			ds.Dispose();
			ds=null;
		}
		
		if (sm != null) {
			sm.Close ();
			sm.Dispose ();
			sm = null;
		}
	}
	
	//Smoothen and Display the Joint Data
	void DisplayJoints ()
	{
		
		for (int i = 0; i < MaxHands; i++) {

			for (int j = 0; j < MaxJoints; j++) {
				if (jointData [i] [j] != null && jointData [i] [j].confidence == 100) {
					smoother3D [i] [j].AddSample (jointData [i] [j].positionWorld);
					myJoints [i] [j].SetActive (true);
					myJoints [i] [j].transform.position = new Vector3 (-1 * smoother3D [i] [j].GetSample ().x, smoother3D [i] [j].GetSample ().y, smoother3D [i] [j].GetSample ().z) * 50f ;			
					jointData [i] [j] = null;
				} else {
					myJoints [i] [j].SetActive (false);
				}
			}

		}
		
		for (int i = 0; i < MaxHands; i++) {

			UpdatePuppetTransform(puppetHands[i], puppetSpotlight[i], myJoints [i] [1], myJoints [i] [10]);
			for (int j = 0; j < MaxJoints; j++) {		

					if (j != 21 && j != 0 && j != 1 && j != 5 && j != 9 && j != 13 && j != 17)
							UpdateBoneTransform (myBones [i] [j], myJoints [i] [j], myJoints [i] [j + 1]);

					UpdateBoneTransform (myBones [i] [21], myJoints [i] [0], myJoints [i] [2]);
					UpdateBoneTransform (myBones [i] [17], myJoints [i] [0], myJoints [i] [18]);

					UpdateBoneTransform (myBones [i] [5], myJoints [i] [14], myJoints [i] [18]);
					UpdateBoneTransform (myBones [i] [9], myJoints [i] [10], myJoints [i] [14]);
					UpdateBoneTransform (myBones [i] [13], myJoints [i] [6], myJoints [i] [10]);
					UpdateBoneTransform (myBones [i] [0], myJoints [i] [2], myJoints [i] [6]);
			}

		}
	}

	public float frontRotationThreshold, frontRotation, sideRotation, puppetY;

	void UpdatePuppetTransform (GameObject _puppet, GameObject _spotlight, GameObject _prevJoint, GameObject _nextJoint)
	{
		
		if (_prevJoint.activeSelf == false || _nextJoint.activeSelf == false){
				_puppet.SetActive (false);
				_spotlight.SetActive (false);
		}else {
			_puppet.SetActive (true);
			_spotlight.SetActive (true);
			
			// Update Position
			Vector3 position = ((_nextJoint.transform.position - _prevJoint.transform.position) / 2f) + _prevJoint.transform.position;
			_puppet.transform.position = new Vector3(position.x, puppetY, 0);
			_spotlight.transform.position = new Vector3(position.x, puppetY + lightY, lightZ);
			//(0,puppetY + lightY, lightZ)
			
			// Update Rotation
			Quaternion rotation = Quaternion.FromToRotation (Vector3.up , _nextJoint.transform.position - _prevJoint.transform.position);

			float rx = rotation.eulerAngles.x, rz = rotation.eulerAngles.z;
			//rotation = Quaternion.LookRotation(_puppet.transform.position - transform.position);

			float ry = rotation.eulerAngles.y;

			rz = rz > 180 ? rz - 360 : rz; 
			rx = rx > 180 ? rx - 360 : rx;

			if (rz > sideRotation)
				rz = sideRotation;
			else if (rz < -sideRotation)
				rz = -sideRotation;
			else
				rz = 0;

			if (rx < -frontRotationThreshold)
				rx = -frontRotation;
			else
				rx = 0;
			
			_puppet.transform.rotation = Quaternion.Euler(new Vector3(rx,0,0));
		}
		
	}

	//Update Bones
	void UpdateBoneTransform (GameObject _bone, GameObject _prevJoint, GameObject _nextJoint)
	{

		if (_prevJoint.activeSelf == false || _nextJoint.activeSelf == false)
			_bone.SetActive (false);
		else {
			_bone.SetActive (true);

			// Update Position
			_bone.transform.position = ((_nextJoint.transform.position - _prevJoint.transform.position) / 2f) + _prevJoint.transform.position;

			// Update Scale
			_bone.transform.localScale = new Vector3 (0.8f, (_nextJoint.transform.position - _prevJoint.transform.position).magnitude - (_prevJoint.transform.position - _nextJoint.transform.position).magnitude / 2f, 0.8f);

			// Update Rotation
			_bone.transform.rotation = Quaternion.FromToRotation (Vector3.up, _nextJoint.transform.position - _prevJoint.transform.position);

		}

	}
	
	//Key inputs to rotate camera and restart
	void RotateCam ()
	{
		Vector3 _RotateAround = new Vector3 (1, 1f, 30f);

		if (_RotateAround != Vector3.zero) {
			if (Input.GetKey (KeyCode.RightArrow))
				transform.RotateAround (_RotateAround, Vector3.up, 200 * Time.deltaTime);

			if (Input.GetKey (KeyCode.LeftArrow))
				transform.RotateAround (_RotateAround, Vector3.up, -200 * Time.deltaTime);
		}

		/* Restart the Level/Refresh Scene */
		if (Input.GetKeyDown (KeyCode.R))
			Application.LoadLevel (0);

		/* Quit the Application */
		if (Input.GetKeyDown (KeyCode.Q))
			Application.Quit ();

	}
	
	//Display Gestures
	void DisplayGestures (PlayerScript _puppet, PXCMHandData.GestureData gestureData)
	{
		if (handList.ContainsKey (gestureData.handId)) {
			txt.text = gestureData.name.ToString ();
			switch (gestureData.name){
				case "spreadfingers":
					_puppet.openMouth();
				break;
				case "fist":
				case "full_pinch":
					_puppet.closeMouth();
				break;
			}
		}
	}
	
	//Process Alerts to keep track of hands for Gesture Display
	void ProcessAlerts (PXCMHandData.AlertData alertData)
	{
		
		if (handList.ContainsKey (alertData.handId)) {
			txt.text = "";
			switch ((PXCMHandData.BodySideType)handList [alertData.handId]) {
			case PXCMHandData.BodySideType.BODY_SIDE_LEFT: 
				break;
			case PXCMHandData.BodySideType.BODY_SIDE_RIGHT:
				break;
			}
		}
		
	}

	private float scale = 1;
	private float lightY = 3f, lightZ = -2.5f;
	//Populate bones and joints gameobjects
	void InitializeGameobjects ()
	{
		myJoints = new GameObject[MaxHands][];
		myBones = new GameObject[MaxHands][];
		jointData = new PXCMHandData.JointData[MaxHands][];		
		for (int i = 0; i < MaxHands; i++) {
			myJoints [i] = new GameObject[MaxJoints];
			myBones [i] = new GameObject[MaxJoints];
			smoother3D [i] = new PXCMDataSmoothing.Smoother3D[MaxJoints];
			jointData [i] = new PXCMHandData.JointData[MaxJoints];
		}

		puppetHands = new GameObject[MaxHands];
		puppetSpotlight = new GameObject[MaxHands];
		puppetBehavior = new PlayerScript[MaxHands];
		GameObject empty = new GameObject ();
		for (int i = 0; i < MaxHands; i++) {

				Vector3 pos = new Vector3(0,puppetY,0);//Vector3.zero
				puppetHands[i] = (GameObject)Instantiate (puppetHandPrefab, pos, Quaternion.identity);
				puppetHands[i].transform.localScale = new Vector3(scale,scale,scale);
				
				pos = new Vector3(0,0, lightZ);
				
				puppetSpotlight[i] = (GameObject)Instantiate (puppetSpotlightPrefab, pos, Quaternion.Euler(new Vector3(45,0,0)));
				
				

				puppetBehavior[i] = puppetHands[i].GetComponent<PlayerScript>();

				for (int j = 0; j < MaxJoints; j++) {
	
						smoother3D [i] [j] = ds.Create3DWeighted (weightsNum);
						jointData [i] [j] = new PXCMHandData.JointData ();
	
						/*if (j == 1)
								myJoints [i] [j] = (GameObject)Instantiate (PalmCenterPrefab, Vector3.zero, Quaternion.identity);
						else if (j == 21 || j == 17 || j == 13 || j == 9 || j == 5)
								myJoints [i] [j] = (GameObject)Instantiate (TipPrefab, Vector3.zero, Quaternion.identity);
						else
								myJoints [i] [j] = (GameObject)Instantiate (JointPrefab, Vector3.zero, Quaternion.identity);

						if (j != 1)
								myBones [i] [j] = (GameObject)Instantiate (BonePrefab, Vector3.zero, Quaternion.identity);
						*/

					myJoints[i][j] = (GameObject)Instantiate (empty, Vector3.zero, Quaternion.identity);

					if (j != 1)
						myBones [i] [j] = (GameObject)Instantiate (empty, Vector3.zero, Quaternion.identity);
				}
		}
	}
	
}
