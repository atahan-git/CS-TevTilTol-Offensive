#pragma strict
//speed x=forward/backward z=sideways
var speed : Vector3 = new Vector3(6, 0, 4);
var jump = 4;
var cam : Transform;
var grounded : boolean;
var maxSpeed = 10;
var airSpeedSlowAmount = 0;

function FixedUpdate () {

	//run and "crouch"
	if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)){
		speed = Vector3(3, 0, 2);
		jump = 4;
	}else if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
		speed = Vector3(8, 0, 6);
		jump = 5;
	}else{
		speed = Vector3(6, 0, 4);
		jump = 4;
	}
	

	//movement
	//input
	var Keyinput = new Vector3(Input.GetAxis("Horizontal") * speed.z, GetComponent.<Rigidbody>().velocity.y, Input.GetAxis("Vertical") * speed.x);
	//while we are touching ground
   	if(grounded == true){
   	var locVel = transform.TransformDirection(Keyinput);
   	locVel = locVel - GetComponent.<Rigidbody>().velocity;
   	if(locVel.x < 0 || locVel.z < 0) locVel /= 2;
	GetComponent.<Rigidbody>().velocity += locVel;
   	}else{
   	//while we are in the air
   	var ForceToAdd = Vector3(Keyinput.x, 0, Keyinput.z);
   	ForceToAdd /= airSpeedSlowAmount;
   	GetComponent.<Rigidbody>().AddRelativeForce(ForceToAdd);
   	}
	//jump
	if(Input.GetKeyDown(KeyCode.Space) && grounded == true){
		GetComponent.<Rigidbody>().velocity.y += jump;
	}
}

//fix vibration after jump
/*function OnTriggerEnter () {
	transform.position.y += 0.0001f;

}*/

//are we touching ground?
function OnCollisionStay (collider : Collision) {

	if(collider.gameObject.tag == "terrain"){
		grounded = true;
	}
}

function OnCollisionExit () {
		grounded = false;
}

