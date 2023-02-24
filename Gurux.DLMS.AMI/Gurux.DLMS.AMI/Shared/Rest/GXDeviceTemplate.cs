﻿//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------
using Gurux.Common;
using System.Runtime.Serialization;
using System.ComponentModel;
using Gurux.DLMS.AMI.Shared.DTOs;
using Gurux.DLMS.AMI.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Gurux.DLMS.AMI.Shared.Rest
{
    /// <summary>
    /// Get device template.
    /// </summary>
    public class GetDeviceTemplateResponse
    {
        /// <summary>
        /// Device template information.
        /// </summary>
        [IncludeSwagger(typeof(GXDeviceTemplateGroup), nameof(GXDeviceTemplateGroup.Id), nameof(GXDeviceTemplateGroup.Name))]
        [IncludeSwagger(typeof(GXObjectTemplate), nameof(GXObjectTemplate.Id), nameof(GXObjectTemplate.Name))]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GXDeviceTemplate Item
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Update device template information. Device template is added if ID is zero.
    /// </summary>
    [DataContract]
    [Description("Add or Update device template information. Device template is added if ID is zero.")]
    public class UpdateDeviceTemplate : IGXRequest<UpdateDeviceTemplateResponse>
    {
        /// <summary>
        /// Inserted or updated device templates.
        /// </summary>
        [DataMember]
        [IncludeSwagger(typeof(GXDeviceTemplateGroup), nameof(GXDeviceTemplateGroup.Id))]
        [ExcludeSwagger(typeof(GXDeviceTemplate), nameof(GXDeviceTemplate.Objects))]
        public GXDeviceTemplate[] Templates
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Device template response.
    /// </summary>
    [DataContract]
    [Description("Insert or update device template response.")]
    public class UpdateDeviceTemplateResponse
    {
        /// <summary>
        /// New device template identifier(s).
        /// </summary>
        [DataMember]
        public Guid[] Ids
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Get list from available device templates.
    /// </summary>
    [DataContract]
    public class ListDeviceTemplates : IGXRequest<ListDeviceTemplatesResponse>
    {
        /// <summary>
        /// Start index.
        /// </summary>
        public int Index
        {
            get;
            set;

        }

        /// <summary>
        /// Amount of the device templates to retreave.
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Filter can be used to filter device templates.
        /// </summary>
        [IncludeSwagger(typeof(GXDeviceTemplateGroup), nameof(GXDeviceTemplateGroup.Id), nameof(GXDeviceTemplateGroup.Name))]
        [ExcludeSwagger(typeof(GXDeviceTemplate), nameof(GXDeviceTemplate.Objects))]
        public GXDeviceTemplate? Filter
        {
            get;
            set;
        }


        /// <summary>
        /// Admin user can access templates from all users.
        /// </summary>
        /// <remarks>
        /// If true, templates from all users are retreaved, not just current user. 
        /// </remarks>
        public bool AllUsers
        {
            get;
            set;
        }

        /// <summary>
        /// Selected extra information.
        /// </summary>
        /// <remarks>
        /// This is reserved for later use.
        /// </remarks>
        public TargetType Select
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Available devices templates response.
    /// </summary>
    [DataContract]
    [Description("Available devices templates response.")]
    public class ListDeviceTemplatesResponse
    {
        /// <summary>
        /// List of device templates.
        /// </summary>
        [DataMember]
        [Description("List of device templates.")]
        [IncludeSwagger(typeof(GXDeviceTemplateGroup), nameof(GXDeviceTemplateGroup.Id), nameof(GXDeviceTemplateGroup.Name))]
        [ExcludeSwagger(typeof(GXDeviceTemplate), nameof(GXDeviceTemplate.Objects))]
        public GXDeviceTemplate[] Templates
        {
            get;
            set;
        }

        /// <summary>
        /// Total count of the device templates.
        /// </summary>
        [DataMember]
        [Description("Total count of the device templates.")]
        public int Count
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Delete device template.
    /// </summary>
    [DataContract]
    public class RemoveDeviceTemplate : IGXRequest<RemoveDeviceTemplateResponse>
    {
        /// <summary>
        /// Removed device identifier(s).
        /// </summary>
        [Description("Removed device identifier(s).")]
        [DataMember]
        public Guid[] Ids
        {
            get;
            set;
        }

        /// <summary>
        /// Items are removed from the database.
        /// </summary>
        /// <remarks>
        /// If false, the Removed date is set for the items, but items are kept on the database.
        /// </remarks>
        [DataMember]
        [Required]
        public bool Delete
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Delete device template response.
    /// </summary>
    [DataContract]
    [Description("Delete device template response.")]
    public class RemoveDeviceTemplateResponse
    {
    }
}
