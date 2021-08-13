using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using FontAwesome.Solid;

namespace PassXYZLib
{
    /// <summary>
    /// This is a class extended PassXYZLib.User. It has a dependency on Xamarin.Forms.
    /// </summary>
    public class PxUser : User
    {
        /// <summary>
        /// This is the icon for the user. We will use an icon to differentiate the user with Device Lock.
        /// </summary>
        public ImageSource ImgSource => new IconSource
        {
            Icon = IsDeviceLockEnabled ? Icon.UserLock : Icon.User
        };
    }
}
