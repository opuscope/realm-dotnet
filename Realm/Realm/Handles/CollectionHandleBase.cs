﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using Realms.Exceptions;
using Realms.Native;

namespace Realms
{
    internal abstract class CollectionHandleBase : NotifiableObjectHandleBase
    {
        public abstract bool IsValid { get; }

        public virtual bool CanSnapshot => false;

        protected CollectionHandleBase(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public abstract int Count();

        public ResultsHandle Snapshot()
        {
            var ptr = SnapshotCore(out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root ?? this, ptr);
        }

        public ResultsHandle GetFilteredResults(string query, RealmValue[] arguments)
        {
            var primitiveValues = new PrimitiveValue[arguments.Length];
            var handles = new RealmValue.HandlesToCleanup?[arguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                if (argument.Type == RealmValueType.Object && !argument.AsRealmObject().IsManaged)
                {
                    throw new RealmException("Can't use unmanaged object as argument of Filter");
                }

                (primitiveValues[i], handles[i]) = argument.ToNative();
            }

            var ptr = GetFilteredResultsCore(query, primitiveValues, out var ex);
            foreach (var argHandles in handles)
            {
                argHandles?.Dispose();
            }

            ex.ThrowIfNecessary();
            return new ResultsHandle(this, ptr);
        }

        public abstract CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle);

        public abstract void Clear();

        protected abstract IntPtr GetFilteredResultsCore(string query, PrimitiveValue[] arguments, out NativeException ex);

        protected virtual IntPtr SnapshotCore(out NativeException ex) => throw new NotSupportedException("Snapshotting this collection is not supported.");
    }
}
