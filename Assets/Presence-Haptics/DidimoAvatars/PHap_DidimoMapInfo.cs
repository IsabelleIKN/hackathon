using UnityEngine;


/*
 * Contains info on disk about how to map SG_HandModelInfo onto a Didimo Avatar 
 * This info is held in a scriptableobject so it can be used across the project.
 * 
 * author
 * max@senseglove.com
 */

[CreateAssetMenu(fileName = "SG_DidimoMapInfo", menuName = "SenseGlove/SG_DidimoMapInfo")]
public class PHap_DidimoMapInfo : ScriptableObject
{
    //TODO: Build the Z offset into the Corrections?

    public Quaternion[] JointCorrections = new Quaternion[0];

    public string sgRawHandInfo = "";

    public void SetJointCorrections(Quaternion[] newCorrections)
    {
        JointCorrections = newCorrections;
    }

    public Quaternion[] GetJointCorrections()
    {
        return JointCorrections;
    }

    public string GetRawSGHandModel()
    {
        return sgRawHandInfo;
    }

    public bool TryGetSGHandModel(out SGCore.Kinematics.BasicHandModel handModel)
    {
        handModel = null;
        if ( !string.IsNullOrEmpty(sgRawHandInfo) )
            handModel = SGCore.Kinematics.BasicHandModel.Deserialize(sgRawHandInfo);   
        return handModel != null;
    }

    public void SetSGHandModel(SGCore.Kinematics.BasicHandModel rawHandModel)
    {
        sgRawHandInfo = rawHandModel.Serialize();
    }

}
