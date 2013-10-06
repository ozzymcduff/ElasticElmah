#region License, Terms and Author(s)

//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-9 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

namespace ElasticElmah.Core.Infrastructure
{
    public sealed class Build
    {
        private Build()
        {
        }

#if DEBUG
        public const bool IsDebug = true;
        public const bool IsRelease = !IsDebug;
        public const string Type = "Debug";
        public const string TypeUppercase = "DEBUG";
        public const string TypeLowercase = "debug";
#else
        public const bool IsDebug = false;
        public const bool IsRelease = !IsDebug;
        public const string Type = "Release";
        public const string TypeUppercase = "RELEASE";
        public const string TypeLowercase = "release";
#endif


        public const string Configuration = TypeLowercase + "; " + Status;

        /// <summary>
        /// Gets a string representing the version of the CLR saved in 
        /// the file containing the manifest. Under 1.0, this returns
        /// the hard-wired string "v1.0.3705".
        /// </summary>
        public static string ImageRuntimeVersion
        {
            get { return typeof (Build).Assembly.ImageRuntimeVersion; }
        }

        /// <summary>
        /// This is the status or milestone of the build. Examples are
        /// M1, M2, ..., Mn, BETA1, BETA2, RC1, RC2, RTM.
        /// </summary>
        public const string Status = "RTM";
    }
}