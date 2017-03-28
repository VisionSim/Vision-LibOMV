/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */


namespace OpenMetaverse
{
    public class CircularQueue<T>
    {
        public readonly T [] Items;

        int _first;
        int _next;
        readonly int _capacity;
        readonly object syncRoot;

        public int First {
            get {
                lock (syncRoot) { return _first; }
            }
        }
        public int Next {
            get {
                lock (syncRoot) { return _next; }
            }
        }

        public CircularQueue (int capacity)
        {
            _capacity = capacity;
            Items = new T [capacity];
            syncRoot = new object ();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="queue">Circular queue to copy</param>
        public CircularQueue (CircularQueue<T> queue)
        {
            lock (queue.syncRoot) {
                _capacity = queue._capacity;
                Items = new T [_capacity];
                syncRoot = new object ();

                for (var i = 0; i < _capacity; i++)
                    Items [i] = queue.Items [i];

                _first = queue._first;
                _next = queue._next;
            }
        }

        public void Clear ()
        {
            lock (syncRoot) {
                // Explicitly remove references to help garbage collection
                for (var i = 0; i < _capacity; i++)
                    Items [i] = default (T);

                _first = _next;
            }
        }

        public void Enqueue (T value)
        {
            lock (syncRoot) {
                Items [_next] = value;
                _next = (_next + 1) % _capacity;
                if (_next == _first) _first = (_first + 1) % _capacity;
            }
        }

        public T Dequeue ()
        {
            lock (syncRoot) {
                var value = Items [_first];
                Items [_first] = default (T);

                if (_first != _next)
                    _first = (_first + 1) % _capacity;

                return value;
            }
        }

        public T DequeueLast ()
        {
            lock (syncRoot) {
                // If the next element is right behind the first element (queue is full),
                // back up the first element by one
                var firstTest = _first - 1;
                if (firstTest < 0) firstTest = _capacity - 1;

                if (firstTest == _next) {
                    --_next;
                    if (_next < 0) _next = _capacity - 1;

                    --_first;
                    if (_first < 0) _first = _capacity - 1;
                } else if (_first != _next) {
                    --_next;
                    if (_next < 0) _next = _capacity - 1;
                }

                var value = Items [_next];
                Items [_next] = default (T);

                return value;
            }
        }
    }
}
