//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Action_Vector2 p_character_controller_JoystickMovement;
        
        private static SteamVR_Action_Boolean p_character_controller_PickObject;
        
        private static SteamVR_Action_Boolean p_character_controller_Jump;
        
        public static SteamVR_Action_Vector2 character_controller_JoystickMovement
        {
            get
            {
                return SteamVR_Actions.p_character_controller_JoystickMovement.GetCopy<SteamVR_Action_Vector2>();
            }
        }
        
        public static SteamVR_Action_Boolean character_controller_PickObject
        {
            get
            {
                return SteamVR_Actions.p_character_controller_PickObject.GetCopy<SteamVR_Action_Boolean>();
            }
        }
        
        public static SteamVR_Action_Boolean character_controller_Jump
        {
            get
            {
                return SteamVR_Actions.p_character_controller_Jump.GetCopy<SteamVR_Action_Boolean>();
            }
        }
        
        private static void InitializeActionArrays()
        {
            Valve.VR.SteamVR_Input.actions = new Valve.VR.SteamVR_Action[] {
                    SteamVR_Actions.character_controller_JoystickMovement,
                    SteamVR_Actions.character_controller_PickObject,
                    SteamVR_Actions.character_controller_Jump};
            Valve.VR.SteamVR_Input.actionsIn = new Valve.VR.ISteamVR_Action_In[] {
                    SteamVR_Actions.character_controller_JoystickMovement,
                    SteamVR_Actions.character_controller_PickObject,
                    SteamVR_Actions.character_controller_Jump};
            Valve.VR.SteamVR_Input.actionsOut = new Valve.VR.ISteamVR_Action_Out[0];
            Valve.VR.SteamVR_Input.actionsVibration = new Valve.VR.SteamVR_Action_Vibration[0];
            Valve.VR.SteamVR_Input.actionsPose = new Valve.VR.SteamVR_Action_Pose[0];
            Valve.VR.SteamVR_Input.actionsBoolean = new Valve.VR.SteamVR_Action_Boolean[] {
                    SteamVR_Actions.character_controller_PickObject,
                    SteamVR_Actions.character_controller_Jump};
            Valve.VR.SteamVR_Input.actionsSingle = new Valve.VR.SteamVR_Action_Single[0];
            Valve.VR.SteamVR_Input.actionsVector2 = new Valve.VR.SteamVR_Action_Vector2[] {
                    SteamVR_Actions.character_controller_JoystickMovement};
            Valve.VR.SteamVR_Input.actionsVector3 = new Valve.VR.SteamVR_Action_Vector3[0];
            Valve.VR.SteamVR_Input.actionsSkeleton = new Valve.VR.SteamVR_Action_Skeleton[0];
            Valve.VR.SteamVR_Input.actionsNonPoseNonSkeletonIn = new Valve.VR.ISteamVR_Action_In[] {
                    SteamVR_Actions.character_controller_JoystickMovement,
                    SteamVR_Actions.character_controller_PickObject,
                    SteamVR_Actions.character_controller_Jump};
        }
        
        private static void PreInitActions()
        {
            SteamVR_Actions.p_character_controller_JoystickMovement = ((SteamVR_Action_Vector2)(SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/character_controller/in/JoystickMovement")));
            SteamVR_Actions.p_character_controller_PickObject = ((SteamVR_Action_Boolean)(SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/character_controller/in/PickObject")));
            SteamVR_Actions.p_character_controller_Jump = ((SteamVR_Action_Boolean)(SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/character_controller/in/Jump")));
        }
    }
}
