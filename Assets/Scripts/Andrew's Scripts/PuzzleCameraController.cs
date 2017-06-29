﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleCameraController : MonoBehaviour {

	private LineRenderer linRen;
	private Vector3 currentTarget;
	private bool isMoving = false;
	private Vector3 destPos;
	private Quaternion facingDir;
	private EventInfo currentEventInfo;

	[SerializeField]
	private GameObject sliders;
	[SerializeField]
	private Toggle lineToggle;

	private Slider sizeSlider;
	private Slider rotateSlider;
	private Slider zoomSlider;

	private float prevSliderValue;
	private float currDegree = 0f;


	private List<VisualizeEvent.DomSnapShot> prevSnapShots;

	void Start() {
		facingDir = Quaternion.LookRotation(new Vector3(0,0,1f));
		if (sliders == null) {
			Debug.LogError ("No reference to the sliders");
		}
		sizeSlider = sliders.transform.Find ("DomSizeSlider").GetComponent<Slider> ();
		rotateSlider = sliders.transform.Find ("RotateSlider").GetComponent<Slider> ();
		zoomSlider = sliders.transform.Find ("ZoomSlider").GetComponent<Slider> ();

		prevSliderValue = sizeSlider.value;

		linRen = GetComponent<LineRenderer> ();
	}


	// Update is called once per frame
	void Update () {
		if (isMoving) {
			if (Vector3.Distance(this.transform.position, destPos) > 50f) {
				Vector3 translationVector = destPos - this.transform.position;
				this.transform.Translate (translationVector * Time.deltaTime, Space.World);
				this.transform.rotation = Quaternion.Slerp (this.transform.rotation, facingDir, Time.deltaTime);
			} else {
				isMoving = false;
			}
		} 

		if (prevSnapShots != null) {
			foreach (VisualizeEvent.DomSnapShot curr in prevSnapShots) {
				if (!curr.Dom.GetComponent<DOMController>().on) {
					curr.Dom.GetComponent<DOMController> ().TurnOn (curr.timeFrac, curr.charge);
				}
			}

			if (!isMoving) {
				AdjustCamera ();
			}

		}


		if (prevSliderValue != sizeSlider.value) {
			prevSliderValue = sizeSlider.value;
			ChangeDomScale ();
		}

		// Draw the line through the data by default for debugging
		if (currentEventInfo != null && lineToggle.isOn) {
			Vector3[] pathPositions = new Vector3[2];
			pathPositions [0] = currentEventInfo.getStart () + new Vector3(3000f, 0, 0);
			pathPositions [1] = currentEventInfo.getEnd () + new Vector3(3000f, 0, 0);
			linRen.SetPositions (pathPositions);
		} else {
			linRen.SetPositions (new Vector3[2]);
		}

	}

	public void MoveCamera(EventInfo currentEventInfo) {

		Vector3 targetPos;
		// If null, move to default position
		if (currentEventInfo == null) {
			targetPos = new Vector3 (3000, -1000, -2000);
		} else {
			targetPos = currentEventInfo.getEventCenterPosition ();
		}


		currentTarget = targetPos;
		ResetSliders ();
		if (prevSnapShots != null) {
			foreach (VisualizeEvent.DomSnapShot curr in prevSnapShots) {
				//curr.Dom.transform.localScale = new Vector3 (1f, 1f, 1f);
				curr.Dom.GetComponent<DOMController> ().TurnOff ();
			}
		}

		destPos = (targetPos - new Vector3 (0, 0, 1000f));
		isMoving = true;
		List<VisualizeEvent.DomSnapShot> snapShots = null;

		if (currentEventInfo != null) {
			snapShots = currentEventInfo.getSnapshot ();
		}

		if (snapShots != null) {
			foreach (VisualizeEvent.DomSnapShot curr in snapShots) {
				curr.Dom.GetComponent<DOMController> ().TurnOn (curr.timeFrac, curr.charge);
			}

			prevSnapShots = snapShots;
			this.currentEventInfo = currentEventInfo;
		}
	}

	public void CleanUp() {
		if (prevSnapShots != null) {
			foreach (VisualizeEvent.DomSnapShot curr in prevSnapShots) {
				//curr.Dom.transform.localScale = new Vector3 (1f, 1f, 1f);
				curr.Dom.GetComponent<DOMController> ().TurnOff ();
			}
			ResetSliders ();
		}

		prevSnapShots = null;

		MoveCamera (null);

	}

	private void ChangeDomScale() {
		foreach (VisualizeEvent.DomSnapShot curr in prevSnapShots) {
			curr.Dom.transform.localScale = (new Vector3 (1f, 1f, 1f) * sizeSlider.value); 
		}
	}

	private void AdjustCamera() {
		float rotateValue = rotateSlider.value;
		float currDistance = Vector3.Distance (this.transform.position, currentTarget);
		float diffToMove = currDistance - zoomSlider.value;

		float rotateDiff = rotateValue - currDegree;
		transform.RotateAround (currentTarget, Vector3.up, rotateDiff);
		transform.LookAt (currentTarget);
		currDegree = rotateValue;


		transform.Translate (this.transform.forward * diffToMove, Space.World);

		currDistance = diffToMove;
	}

	private void ResetSliders() {
		sizeSlider.value = 1.0f;
		rotateSlider.value = 0f;
		zoomSlider.value = 1000f;
	}
}
