﻿// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Utils
{
    using System.Reflection.Metadata;

    /// <summary>
    /// A class that contains constants to be used accross the entire project.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Proprietatea lui Darius Timoce. Cine sterge sau modifica aici o sa faca validatoare tot restul proiectului.
        /// </summary>
        public const string CONNECTIONSTRINGDARIUS = "Data Source=DESKTOP-2F4KVKB;Initial Catalog=SteampunksDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

        /// <summary>
        /// Connection string for Sonia.
        /// </summary>
        public const string CONNECTIONSTRINGSONIA = "Data Source=DESKTOP-J4L4KLR\\SQLEXPRESS;Initial Catalog=SteampunksDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

        /// <summary>
        /// Connection string for Ilinca.
        /// </summary>
        public const string CONNECTIONSTRINGILINCA = "Server=localhost\\SQLEXPRESS;Database=SteampunksDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
        
        public const string CONNECTIONSTRINGBOGDAN = "Data Source=BOGDY;Initial Catalog=SteampunksDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
       
        /// <summary>
        /// Connection string for Bianca.
        /// </summary>
        public const string CONNECTIONSTRINGBIANCA = "Data Source=DESKTOP-3UMPJAT\\SQLEXPRESS;Initial Catalog=SteampunksDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
    }
}
