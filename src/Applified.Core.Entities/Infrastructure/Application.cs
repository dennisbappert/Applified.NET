#region Copyright (C) 2014 Applified.NET 
// Copyright (C) 2014 Applified.NET
// http://www.applified.net

// This file is part of Applified.NET.

// Applified.NET is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.

// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Applified.Core.Entities.Contracts;

namespace Applified.Core.Entities.Infrastructure
{
    [Table("Applications")]
    public class Application : IGuidKey
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(128)]
        public string AccessToken { get; set; }

        public ICollection<Binding> Bindings { get; set; }

        public ICollection<Deployment> Deployments { get; set; }

        public Guid? ActiveDeploymentId { get; set; }

        [ForeignKey("ActiveDeploymentId")]
        public Deployment ActiveDeployment { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
