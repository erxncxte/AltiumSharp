﻿using System;
using System.Collections.Generic;
using System.Linq;
using AltiumSharp.BasicTypes;

namespace AltiumSharp.Records
{
    public class SchPrimitive : Primitive, IContainer
    {
        public virtual int Record { get; }

        public bool IsNotAccesible { get; set; }

        public int IndexInSheet => (Owner as SchComponent)?.GetPrimitiveIndexOf(this) ?? -1;

        internal int OwnerIndex { get; set; }

        public int OwnerPartId { get; set; }

        public int OwnerPartDisplayMode { get; set; }

        public bool GraphicallyLocked { get; set; }

        private List<SchPrimitive> _primitives = new List<SchPrimitive>();
        internal IReadOnlyList<SchPrimitive> Primitives => _primitives;

        public override bool IsVisible =>
            base.IsVisible && ((Owner as SchComponent)?.DisplayMode ?? 0) == OwnerPartDisplayMode;

        public SchPrimitive() : base()
        {
            OwnerPartId = -1;
        }

        public IEnumerable<T> GetPrimitivesOfType<T>(bool flatten = true) where T : Primitive
        {
            if (flatten)
            {
                return Enumerable.Concat(
                    GetPrimitivesOfType<T>(false),
                    Primitives.SelectMany(p => p.GetPrimitivesOfType<T>(true)));
            }
            else
            {
                return GetAllPrimitives().OfType<T>();
            }
        }

        protected int GetPrimitiveIndexOf(SchPrimitive primitive) =>
            _primitives.IndexOf(primitive);

        public override CoordRect CalculateBounds() => CoordRect.Empty;

        public virtual void ImportFromParameters(ParameterCollection p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));

            var recordType = p["RECORD"].AsIntOrDefault();
            if (recordType != Record) throw new ArgumentException($"Record type mismatch when deserializing. Expected {Record} but got {recordType}", nameof(p));

            OwnerIndex = p["OWNERINDEX"].AsIntOrDefault();
            IsNotAccesible = p["ISNOTACCESIBLE"].AsBool();
            OwnerPartId = p["OWNERPARTID"].AsIntOrDefault();
            OwnerPartDisplayMode = p["OWNERPARTDISPLAYMODE"].AsIntOrDefault();
            GraphicallyLocked = p["GRAPHICALLYLOCKED"].AsBool();
        }

        public virtual void ExportToParameters(ParameterCollection p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));

            p.Add("RECORD", Record);
            p.Add("OWNERINDEX", OwnerIndex);
            p.Add("ISNOTACCESIBLE", IsNotAccesible);
            p.Add("INDEXINSHEET", IndexInSheet);
            p.Add("OWNERPARTID", OwnerPartId);
            p.Add("OWNERPARTDISPLAYMODE", OwnerPartDisplayMode);
            p.Add("GRAPHICALLYLOCKED", GraphicallyLocked);
        }

        public ParameterCollection ExportToParameters()
        {
            var parameters = new ParameterCollection();
            ExportToParameters(parameters);
            return parameters;
        }

        public IEnumerable<SchPrimitive> GetAllPrimitives()
        {
            return Enumerable.Concat(Primitives, DoGetParameters());
        }

        protected virtual IEnumerable<SchPrimitive> DoGetParameters()
        {
            return Enumerable.Empty<SchPrimitive>();
        }

        public void Add(SchPrimitive primitive)
        {
            if (primitive == null) throw new ArgumentNullException(nameof(primitive));
            if (primitive == this) return;

            if (DoAdd(primitive))
            {
                primitive.Owner = this;
                _primitives.Add(primitive);
            }
        }

        protected virtual bool DoAdd(SchPrimitive primitive)
        {
            return true;
        }

        public void Remove(SchPrimitive primitive)
        {
            if (primitive == null) throw new ArgumentNullException(nameof(primitive));

            primitive.Owner = null;
            _primitives.Remove(primitive);
        }
    }
}