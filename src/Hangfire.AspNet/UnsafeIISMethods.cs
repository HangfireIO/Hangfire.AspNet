// <copyright file="UnsafeIISMethods.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Reflection;

// ReSharper disable InconsistentNaming

namespace Hangfire.AspNet
{
    internal static class UnsafeIISMethods
    {
        private static readonly Lazy<UnsafeIISMethodsWrapper> IIS = new Lazy<UnsafeIISMethodsWrapper>(() => new UnsafeIISMethodsWrapper());

        public static bool RequestedAppDomainRestart
        {
            get
            {
                if (IIS.Value.CheckConfigChanged == null)
                {
                    return false;
                }

                return !IIS.Value.CheckConfigChanged();
            }
        }
        
        private class UnsafeIISMethodsWrapper
        {
            public UnsafeIISMethodsWrapper()
            {
                // Private reflection to get the UnsafeIISMethods
                var type = Type.GetType("System.Web.Hosting.UnsafeIISMethods, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                // This method can tell us if ASP.NET requested and app domain shutdown
                var methodInfo = type?.GetMethod("MgdHasConfigChanged", BindingFlags.NonPublic | BindingFlags.Static);

                if (methodInfo == null)
                {
                    // Method signature changed so just bail
                    return;
                }

                try
                {
                    CheckConfigChanged = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), methodInfo);
                }
                catch (ArgumentException)
                {
                }
                catch (MissingMethodException)
                {
                }
                catch (MethodAccessException)
                {
                }
                // If we failed to create the delegate we can't do the check reliably
            }

            public Func<bool> CheckConfigChanged { get; }
        }
    }
}