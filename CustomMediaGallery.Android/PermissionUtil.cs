using System;
using Android.Content.PM;

namespace CustomMediaGallery.Droid
{
    public static class PermissionUtil
    {
        /**
		* Check that all given permissions have been granted by verifying that each entry in the
		* given array is of the value Permission.Granted.
		*
		* See Activity#onRequestPermissionsResult (int, String[], int[])
		*/
        public static bool VerifyPermissions(Permission[] grantResults)
        {
            // At least one result must be checked.
            if (grantResults.Length < 1)
                return false;

            // Verify that each required permission has been granted, otherwise return false.
            foreach (Permission result in grantResults)
            {
                if (result != Permission.Granted)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
