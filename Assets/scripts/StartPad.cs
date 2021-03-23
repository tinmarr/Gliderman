using System.Collections;
using UnityEngine;

public class StartPad : MonoBehaviour
{
    public GliderController player;
    public float startStrengthUp;
    public float startStrengthForward;
    public float throttling = 0.1f;
    public HotkeyConfig hotkeys;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(hotkeys.launchFromPlatform))
        {
            player.GetRB().isKinematic = false;
            Vector3 finalForce = (player.transform.forward * startStrengthForward) + (player.transform.up * startStrengthUp);
            Vector3 currentForce = Vector3.zero;
            for (int i = 0; i < 1/throttling; i++)
            {
                currentForce = Vector3.Lerp(currentForce, finalForce, throttling);
                StartCoroutine(Launch(currentForce, throttling * i));

                if (!(i + 1 < 1 / throttling))
                {

                }
            }
        }
    }

    public IEnumerator Launch(Vector3 thrust, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        player.GetRB().AddForce(thrust);
    }

    public IEnumerator SetLaunched()
    {
        yield return new WaitForSeconds(1f);
        player.SetLaunched(true);
    }

    private void OnValidate()
    {
        startStrengthUp = Mathf.Clamp(startStrengthUp, 0, Mathf.Infinity);
        startStrengthForward = Mathf.Clamp(startStrengthForward, 0, Mathf.Infinity);
    }
}
