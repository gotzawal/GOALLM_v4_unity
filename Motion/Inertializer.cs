using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static TransformationUtil;

[RequireComponent(typeof(Animator))]
public class Inertializer : MonoBehaviour
{
    public enum EvaluateSpace
    {
        Local,
        Character,
        World
    }


    struct TransformState
    {
        public Vector3 Pos;
        public Vector3 PosVel;

        public quaternion Rot;
        public Vector3 RotVel;
    }


    private Animator _Animator;
    public Animator animator
    {
        get
        {
            if (_Animator == null)
                _Animator = GetComponent<Animator>();
            return _Animator;
        }
    }


    public AvatarMask AvatarMask;

    public float HalfLife = 0.5f;


    #region AvatarMaskBodyPart Mapping
    public static readonly Dictionary<AvatarMaskBodyPart, HumanBodyBones[]> AvatarBodyPartMapping = new Dictionary<AvatarMaskBodyPart, HumanBodyBones[]>() {
            { AvatarMaskBodyPart.Body, new HumanBodyBones[]{
                HumanBodyBones.Chest,
                HumanBodyBones.Hips,
                HumanBodyBones.Spine,
                HumanBodyBones.UpperChest } },
            { AvatarMaskBodyPart.Head, new HumanBodyBones[]{
                HumanBodyBones.Head,
                HumanBodyBones.Neck,
                HumanBodyBones.LeftEye,
                HumanBodyBones.RightEye,
                HumanBodyBones.Jaw } },
            { AvatarMaskBodyPart.LeftArm, new HumanBodyBones[]{
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand} },
            { AvatarMaskBodyPart.RightArm, new HumanBodyBones[]{
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand} },
            { AvatarMaskBodyPart.LeftLeg, new HumanBodyBones[]{
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.LeftToes } },
            { AvatarMaskBodyPart.RightLeg, new HumanBodyBones[]{
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot,
                HumanBodyBones.RightToes } },
            { AvatarMaskBodyPart.LeftFingers, new HumanBodyBones[] {
                HumanBodyBones.LeftThumbProximal,
                HumanBodyBones.LeftThumbIntermediate,
                HumanBodyBones.LeftThumbDistal,
                HumanBodyBones.LeftIndexProximal,
                HumanBodyBones.LeftIndexIntermediate,
                HumanBodyBones.LeftIndexDistal,
                HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftMiddleIntermediate,
                HumanBodyBones.LeftMiddleDistal,
                HumanBodyBones.LeftRingProximal,
                HumanBodyBones.LeftRingIntermediate,
                HumanBodyBones.LeftRingDistal,
                HumanBodyBones.LeftLittleProximal,
                HumanBodyBones.LeftLittleIntermediate,
                HumanBodyBones.LeftLittleDistal } },
            { AvatarMaskBodyPart.RightFingers, new HumanBodyBones[] {
                HumanBodyBones.RightThumbProximal,
                HumanBodyBones.RightThumbIntermediate,
                HumanBodyBones.RightThumbDistal,
                HumanBodyBones.RightIndexProximal,
                HumanBodyBones.RightIndexIntermediate,
                HumanBodyBones.RightIndexDistal,
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightMiddleIntermediate,
                HumanBodyBones.RightMiddleDistal,
                HumanBodyBones.RightRingProximal,
                HumanBodyBones.RightRingIntermediate,
                HumanBodyBones.RightRingDistal,
                HumanBodyBones.RightLittleProximal,
                HumanBodyBones.RightLittleIntermediate,
                HumanBodyBones.RightLittleDistal } },
        };
    #endregion

    Transform[] CollectTransforms()
    {
        HashSet<Transform> xforms = new HashSet<Transform>();

        if (AvatarMask != null)
        {
            for (int i = 0; i < AvatarMask.transformCount; ++i)
            {
                string path = AvatarMask.GetTransformPath(i);
                var xform = transform.Find(path);
                if (xform != null)
                    xforms.Add(xform);
            }
        }

        for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; ++i)
        {
            bool active = AvatarMask == null ? true : AvatarMask.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i);

            HumanBodyBones[] hbbones;
            if (!AvatarBodyPartMapping.TryGetValue((AvatarMaskBodyPart)i, out hbbones))
                continue;
            foreach (var hbb in hbbones)
            {
                var xform = animator.GetBoneTransform(hbb);
                if (xform == null)
                    continue;

                if (active)
                    xforms.Add(xform);
                else
                    xforms.Remove(xform);
            }
        }

        xforms.Remove(transform);

        return SortParentToChild(xforms);
    }

    Transform[] SortParentToChild(IEnumerable<Transform> transforms)
    {
        return transforms.OrderBy(t => GetHierarchyDepth(t)).ToArray();
    }

    int GetHierarchyDepth(Transform transform)
    {
        int depth = 0;
        while (transform.parent != null)
        {
            depth++;
            transform = transform.parent;
        }
        return depth;
    }

    Transform[] Transforms;
    Vector3[] prevPos;
    Quaternion[] prevRot;
    TransformState[] OffsetStates;


    public void InitializeInertializer()
    {
        Transforms = CollectTransforms();
        prevPos = new Vector3[Transforms.Length];
        prevRot = new Quaternion[Transforms.Length];
        OffsetStates = new TransformState[Transforms.Length];
        for (int i = 0; i < Transforms.Length; i++)
        {
            prevPos[i] = Transforms[i].localPosition;
            prevRot[i] = Transforms[i].localRotation;
            OffsetStates[i] = new TransformState
            {
                Pos = Vector3.zero,
                PosVel = Vector3.zero,
                Rot = Quaternion.identity,
                RotVel = Vector3.zero
            };
        }
    }

    public void LateUpdate()
    {
        // damp offset every time step
        DampOffsetStates(Time.deltaTime);

        for (int i = 0; i < Transforms.Length; ++i)
        {
            prevPos[i] = Transforms[i].localPosition;
            prevRot[i] = Transforms[i].localRotation;
            Transforms[i].localPosition = Transforms[i].localPosition + OffsetStates[i].Pos;
            Transforms[i].localRotation = OffsetStates[i].Rot * Transforms[i].localRotation;
        }
    }


    public void InertializedTransition(AnimationClip clip, float normalizedTime)
    {
        int count = Transforms.Length;
        Vector3[] beforePositions = new Vector3[count];
        Quaternion[] beforeRotations = new Quaternion[count];
        Vector3[] beforeVelocitiesPos = new Vector3[count];
        Vector3[] beforeVelocitiesRot = new Vector3[count];

        float deltaTime = Mathf.Max(Time.deltaTime, 1e-5f);
        animator.Update(0f); // Force Animator update
        for (int i = 0; i < count; ++i)
        {
            Transform t = Transforms[i];
            beforePositions[i] = t.localPosition;
            beforeRotations[i] = t.localRotation;

            beforeVelocitiesPos[i] = (t.localPosition - prevPos[i]) / deltaTime;
            beforeVelocitiesRot[i] = GetAngularVelocity(prevRot[i], t.localRotation, deltaTime);
        }

        Vector3[] afterPositions = new Vector3[count];
        Quaternion[] afterRotations = new Quaternion[count];
        Vector3[] afterVelocitiesPos = new Vector3[count];
        Vector3[] afterVelocitiesRot = new Vector3[count];


        float clipLength = clip.length;
        float dt = 1f / 30f;
        float normalizedInterval = dt / clipLength;

        animator.Play(clip.name, 0, normalizedTime + normalizedInterval);
        animator.Update(0f); // Force Animator update

        Vector3[] positionsAtTPlusDt = new Vector3[count];
        Quaternion[] rotationsAtTPlusDt = new Quaternion[count];
        for (int i = 0; i < count; ++i)
        {
            Transform t = Transforms[i];
            positionsAtTPlusDt[i] = t.localPosition;
            rotationsAtTPlusDt[i] = t.localRotation;
        }

        animator.Play(clip.name, 0, normalizedTime);
        animator.Update(0f); // Force Animator update

        for (int i = 0; i < count; ++i)
        {
            Transform t = Transforms[i];
            afterPositions[i] = t.localPosition;
            afterRotations[i] = t.localRotation;
        }

        for (int i = 0; i < count; ++i)
        {
            afterVelocitiesPos[i] = (positionsAtTPlusDt[i] - afterPositions[i]) / dt;
            afterVelocitiesRot[i] = GetAngularVelocity(afterRotations[i], rotationsAtTPlusDt[i], dt);
        }

        // update offsets
        for (int i = 0; i < count; ++i)
        {
            OffsetStates[i].Pos += beforePositions[i] - afterPositions[i];
            OffsetStates[i].PosVel += beforeVelocitiesPos[i] - afterVelocitiesPos[i];
            OffsetStates[i].Rot = OffsetStates[i].Rot * beforeRotations[i] * Quaternion.Inverse(afterRotations[i]);
            OffsetStates[i].RotVel += beforeVelocitiesRot[i] - afterVelocitiesRot[i];
        }

    }
    void DampOffsetStates(float deltaTime)
    {
        float y = 2f * 0.6931f / HalfLife;
        float eydt = Mathf.Exp(-y * deltaTime);

        for (int i = 0; i < OffsetStates.Length; i++)
        {
            TransformState state = OffsetStates[i];

            // Damping Position Offsets
            Vector3 j1 = state.PosVel + state.Pos * y;

            state.Pos = eydt * (state.Pos + j1 * deltaTime);
            state.PosVel = eydt * (state.PosVel - j1 * y * deltaTime);

            // Damping Rotation Offsets
            Vector3 j0 = QuaternionToScaledAngleAxis(state.Rot);
            Vector3 j1_rot = state.RotVel + j0 * y;

            Vector3 new_j0 = eydt * (j0 + j1_rot * deltaTime);
            state.Rot = ScaledAngleAxisToQuaternion(new_j0);

            state.RotVel = eydt * (state.RotVel - j1_rot * y * deltaTime);

            // Update the OffsetStates array
            OffsetStates[i] = state;
        }
    }

}


