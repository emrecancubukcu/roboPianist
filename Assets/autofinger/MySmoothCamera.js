#pragma strict

var target : Transform;
var offset = -Vector3.forward;
var moveSpeed = 1.0f;
var rotSpeed = 360.0f;
 
function LateUpdate(){
  var wantedPos = target.position + target.rotation * offset;
 
  transform.position = Vector3.Lerp(transform.position, wantedPos, Time.deltaTime*moveSpeed);
  transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), Time.deltaTime*rotSpeed);
}
