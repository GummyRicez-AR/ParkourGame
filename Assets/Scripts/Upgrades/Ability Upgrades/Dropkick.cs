using UnityEngine;

public class Dropkick : MonoBehaviour
{
    private PlayerController player;
    private bool dropKickedDuringJump;
    public float dropkickBoost = 15;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        dropKickedDuringJump = false;
        player = GetComponentInParent<PlayerController>();
        player.Slide.AddListener(DropkickFunction);
        player.LandOnGround.AddListener(OnGrounded);
    }

    private void OnDestroy()
    {
        player.Slide.RemoveListener(DropkickFunction);
        player.LandOnGround.RemoveListener(OnGrounded);
    }

    private void DropkickFunction()
    {
        if (!player.Grounded && !dropKickedDuringJump)
        {
            player.AddLinearVelocity(Vector3.forward, dropkickBoost / 2f);
            player.SetLinearVelocity(Vector3.up, player.GetMovementVector().y / 1.5f);
            player.AddTempWallBoost(dropkickBoost);
            player.GetComponent<AudioSource>().PlayOneShot(player.wooshSfx);
            dropKickedDuringJump = true;
        }
    }

    private void OnGrounded()
    {
        dropKickedDuringJump = false;
    }
}
