
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : MonoBehaviour
{

    public class Contact
    {
        public string clipName;
        public float start;
        public float end;
        public float rotationAngle;
        public AnimationClip clip;

        public Contact(string clipName, float start, float end, float rotationAngle)
        {
            this.clipName = clipName;
            this.start = start;
            this.end = end;
            this.rotationAngle = rotationAngle;
        }
        public override string ToString()
        {
            return $"Clip Name: {clipName}, Start: {start}, End: {end}, Rotation Angle: {rotationAngle}";
        }
    }

    private List<Contact> leftToRightContacts = new List<Contact>
    {
        // Original contacts with "walk" clip
        new Contact("walk", 1827f, 1846f, -56.25702f),
        new Contact("walk", 1876f, 1915f, -22.34644f),
        new Contact("walk", 1944f, 1967f, 25.1137f),
        new Contact("walk", 1985f, 2014f, 66.21126f),
        new Contact("walk", 2049f, 2082f, -2.604919f),
        new Contact("walk", 2114f, 2135f, -49.99588f),
        new Contact("walk", 2171f, 2207f, -10.4993f),
        new Contact("walk", 2240f, 2269f, -3.714478f),
        new Contact("walk", 2291f, 2313f, -45.4639f),
        new Contact("walk", 2337f, 2375f, 8.527441f),
        new Contact("walk", 2407f, 2440f, -37.59225f),
        new Contact("walk", 2471f, 2503f, -29.25766f),
        new Contact("walk", 2533f, 2565f, -42.28784f),
        new Contact("walk", 2595f, 2626f, -48.8009f),
        new Contact("walk", 2654f, 2684f, -63.64676f),
        new Contact("walk", 2713f, 2743f, -87.15045f),
        new Contact("walk", 2769f, 2803f, -114.2444f),
        new Contact("walk", 2833f, 2864f, 38.1627f),
        new Contact("walk", 2901f, 2929f, 51.63437f),
        new Contact("walk", 2964f, 2993f, 43.61151f),
        new Contact("walk", 3025f, 3053f, 47.76364f),
        new Contact("walk", 3087f, 3115f, 37.49245f),
        new Contact("walk", 3147f, 3177f, 36.87049f),
        new Contact("walk", 3207f, 3237f, 51.00439f),
        new Contact("walk", 3267f, 3299f, 50.48861f),

        // Mirrored contacts from rightToLeftContacts
        new Contact("walk_mirrored", 1803f, 1827f, 13.595f),
        new Contact("walk_mirrored", 1846f, 1876f, 91.38818f),
        new Contact("walk_mirrored", 1915f, 1944f, -3.552176f),
        new Contact("walk_mirrored", 1967f, 1985f, -63.15828f),
        new Contact("walk_mirrored", 2014f, 2049f, -19.15013f),
        //new Contact("walk_mirrored", 2082f, 2114f, 0.9721375f),
        new Contact("walk_mirrored", 2135f, 2171f, 110.903f),
        new Contact("walk_mirrored", 2207f, 2240f, 6.523834f),
        new Contact("walk_mirrored", 2269f, 2291f, 29.61008f),
        new Contact("walk_mirrored", 2313f, 2337f, 46.44849f),
        new Contact("walk_mirrored", 2375f, 2407f, 28.84488f),
        new Contact("walk_mirrored", 2440f, 2471f, 29.75867f),
        new Contact("walk_mirrored", 2503f, 2533f, 38.04196f),
        new Contact("walk_mirrored", 2565f, 2595f, 40.06995f),
        new Contact("walk_mirrored", 2626f, 2654f, 47.18994f),
        new Contact("walk_mirrored", 2684f, 2713f, 61.70096f),
        new Contact("walk_mirrored", 2743f, 2769f, 83.12799f),
        new Contact("walk_mirrored", 2803f, 2833f, 60.13873f),
        new Contact("walk_mirrored", 2864f, 2901f, -94.87385f),
        new Contact("walk_mirrored", 2929f, 2964f, -57.60403f),
        new Contact("walk_mirrored", 2993f, 3025f, -46.0688f),
        new Contact("walk_mirrored", 3053f, 3087f, -40.75135f),
        new Contact("walk_mirrored", 3115f, 3147f, -45.43167f),
        new Contact("walk_mirrored", 3177f, 3207f, -46.81852f),
        new Contact("walk_mirrored", 3237f, 3267f, -64.81129f)
    };

    private List<Contact> rightToLeftContacts = new List<Contact>
    {
        // Original contacts with "walk" clip
        new Contact("walk", 1803f, 1827f, -13.595f),
        new Contact("walk", 1846f, 1876f, -91.38818f),
        new Contact("walk", 1915f, 1944f, 3.552176f),
        new Contact("walk", 1967f, 1985f, 63.15828f),
        new Contact("walk", 2014f, 2049f, 19.15013f),
        //new Contact("walk", 2082f, 2114f, -0.9721375f),
        new Contact("walk", 2135f, 2171f, -110.903f),
        new Contact("walk", 2207f, 2240f, -6.523834f),
        new Contact("walk", 2269f, 2291f, -29.61008f),
        new Contact("walk", 2313f, 2337f, -46.44849f),
        new Contact("walk", 2375f, 2407f, -28.84488f),
        new Contact("walk", 2440f, 2471f, -29.75867f),
        new Contact("walk", 2503f, 2533f, -38.04196f),
        new Contact("walk", 2565f, 2595f, -40.06995f),
        new Contact("walk", 2626f, 2654f, -47.18994f),
        new Contact("walk", 2684f, 2713f, -61.70096f),
        new Contact("walk", 2743f, 2769f, -83.12799f),
        new Contact("walk", 2803f, 2833f, -60.13873f),
        new Contact("walk", 2864f, 2901f, 94.87385f),
        new Contact("walk", 2929f, 2964f, 57.60403f),
        new Contact("walk", 2993f, 3025f, 46.0688f),
        new Contact("walk", 3053f, 3087f, 40.75135f),
        new Contact("walk", 3115f, 3147f, 45.43167f),
        new Contact("walk", 3177f, 3207f, 46.81852f),
        new Contact("walk", 3237f, 3267f, 64.81129f),

        // Mirrored contacts from leftToRightContacts
        new Contact("walk_mirrored", 1827f, 1846f, 56.25702f),
        new Contact("walk_mirrored", 1876f, 1915f, 22.34644f),
        new Contact("walk_mirrored", 1944f, 1967f, -25.1137f),
        new Contact("walk_mirrored", 1985f, 2014f, -66.21126f),
        new Contact("walk_mirrored", 2049f, 2082f, 2.604919f),
        new Contact("walk_mirrored", 2114f, 2135f, 49.99588f),
        new Contact("walk_mirrored", 2171f, 2207f, 10.4993f),
        new Contact("walk_mirrored", 2240f, 2269f, 3.714478f),
        new Contact("walk_mirrored", 2291f, 2313f, 45.4639f),
        new Contact("walk_mirrored", 2337f, 2375f, -8.527441f),
        new Contact("walk_mirrored", 2407f, 2440f, 37.59225f),
        new Contact("walk_mirrored", 2471f, 2503f, 29.25766f),
        new Contact("walk_mirrored", 2533f, 2565f, 42.28784f),
        new Contact("walk_mirrored", 2595f, 2626f, 48.8009f),
        new Contact("walk_mirrored", 2654f, 2684f, 63.64676f),
        new Contact("walk_mirrored", 2713f, 2743f, 87.15045f),
        new Contact("walk_mirrored", 2769f, 2803f, 114.2444f),
        new Contact("walk_mirrored", 2833f, 2864f, -38.1627f),
        new Contact("walk_mirrored", 2901f, 2929f, -51.63437f),
        new Contact("walk_mirrored", 2964f, 2993f, -43.61151f),
        new Contact("walk_mirrored", 3025f, 3053f, -47.76364f),
        new Contact("walk_mirrored", 3087f, 3115f, -37.49245f),
        new Contact("walk_mirrored", 3147f, 3177f, -36.87049f),
        new Contact("walk_mirrored", 3207f, 3237f, -51.00439f),
        new Contact("walk_mirrored", 3267f, 3299f, -50.48861f)
    };


    private CharacterControl charactercontrol;
    private Vector3 rootMotionDeltaPosition;
    private Quaternion rootMotionDeltaRotation;
    private float frame;
    private bool isLeftToRight;
    private Contact currentContact;
    private bool locomotionStarted;

    void Start()
    {
        charactercontrol = GetComponent<CharacterControl>();
        if (charactercontrol == null)
        {
            Debug.LogError("Locomotion: CharacterControl 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        locomotionStarted = false;
        isLeftToRight = true;

        foreach (Contact contact in leftToRightContacts)
        {
            contact.clip = charactercontrol.FindAnimationClip(contact.clipName);
            if (contact.clip == null)
            {
                Debug.LogWarning($"Locomotion: Animation clip '{contact.clipName}' not found.");
            }
        }
        foreach (Contact contact in rightToLeftContacts)
        {
            contact.clip = charactercontrol.FindAnimationClip(contact.clipName);
            if (contact.clip == null)
            {
                Debug.LogWarning($"Locomotion: Animation clip '{contact.clipName}' not found.");
            }
        }
    }

    void Update()
    {
        // motionType을 사용하여 걷기 상태를 감지
        if (charactercontrol.motionType != "walk")
        {
            locomotionStarted = false;
            frame = 0f; // 프레임 초기화
            return;
        }
        if (!locomotionStarted)
        {
            currentContact = FindBestContact();
            frame = currentContact.start;
            charactercontrol.PlayClipFromFrame(currentContact.clip, frame);
            locomotionStarted = true;
        }
        if (frame >= currentContact.end)
        {
            SetNextContact();
        }
        frame += Time.deltaTime * 60f;
    }

    private void SetNextContact()
    {
        float exceed = frame - currentContact.end;

        // 좌우 교대
        isLeftToRight = !isLeftToRight;

        // 필요한 회전에 가장 잘 맞는 Contact 찾기
        currentContact = FindBestContact();

        frame = currentContact.start + exceed;

        charactercontrol.PlayClipFromFrame(currentContact.clip, frame);
    }

    private Contact FindBestContact()
    {
        Vector3 directionToTarget = (charactercontrol.targetPoint - transform.position).normalized; // walkTargetPoint 대신 targetPoint 사용
        float angleToTarget = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        float currentYRotation = transform.eulerAngles.y;
        float neededRotation = Mathf.DeltaAngle(currentYRotation, angleToTarget);

        List<Contact> contacts = isLeftToRight ? leftToRightContacts : rightToLeftContacts;

        Contact bestContact = contacts[0];
        float smallestAngleDifference = Mathf.Infinity;

        foreach (var contact in contacts)
        {
            // Contact의 회전 각도는 해당 Contact 동안 캐릭터가 회전할 각도
            float angleDifference = Mathf.Abs(neededRotation - contact.rotationAngle);
            if (angleDifference < smallestAngleDifference)
            {
                smallestAngleDifference = angleDifference;
                bestContact = contact;
            }
        }
        Debug.Log($"Locomotion: Selected Contact -> {bestContact.ToString()}");
        return bestContact;
    }
}