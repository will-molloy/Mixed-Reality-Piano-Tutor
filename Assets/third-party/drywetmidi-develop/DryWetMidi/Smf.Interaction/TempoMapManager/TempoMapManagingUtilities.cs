﻿using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    ///     Extension methods for managing tempo map.
    /// </summary>
    public static class TempoMapManagingUtilities
    {
        #region Methods

        /// <summary>
        ///     Creates an instance of the <see cref="TempoMapManager" /> initializing it with the
        ///     specified events collections and time division.
        /// </summary>
        /// <param name="eventsCollections">
        ///     Collection of <see cref="EventsCollection" /> which hold events
        ///     that represent tempo map of a MIDI file.
        /// </param>
        /// <param name="timeDivision">
        ///     MIDI file time division which specifies the meaning of the time
        ///     used by events of the file.
        /// </param>
        /// <returns>
        ///     An instance of the <see cref="TempoMapManager" /> that can be used to manage
        ///     tempo map represented by the <paramref name="eventsCollections" /> and <paramref name="timeDivision" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventsCollections" /> is null. -or-
        ///     <paramref name="timeDivision" /> is null.
        /// </exception>
        public static TempoMapManager ManageTempoMap(this IEnumerable<EventsCollection> eventsCollections,
            TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return new TempoMapManager(timeDivision, eventsCollections);
        }

        /// <summary>
        ///     Creates an instance of the <see cref="TempoMapManager" /> initializing it with the
        ///     specified time division and events collections of the specified track chunks.
        /// </summary>
        /// <param name="trackChunks">
        ///     Collection of <see cref="TrackChunk" /> which hold events
        ///     that represent tempo map of a MIDI file.
        /// </param>
        /// <param name="timeDivision">
        ///     MIDI file time division which specifies the meaning of the time
        ///     used by events of the file.
        /// </param>
        /// <returns>
        ///     An instance of the <see cref="TempoMapManager" /> that can be used to manage
        ///     tempo map represented by the <paramref name="trackChunks" /> and <paramref name="timeDivision" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="trackChunks" /> is null. -or-
        ///     <paramref name="timeDivision" /> is null.
        /// </exception>
        public static TempoMapManager ManageTempoMap(this IEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return trackChunks.Select(c => c.Events).ManageTempoMap(timeDivision);
        }

        /// <summary>
        ///     Creates an instance of the <see cref="TempoMapManager" /> initializing it with the
        ///     events collections of the specified MIDI file.
        /// </summary>
        /// <param name="file">MIDI file to manage tempo map of.</param>
        /// <returns>
        ///     An instance of the <see cref="TempoMapManager" /> that can be used to manage
        ///     tempo map of the <paramref name="file" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="file" /> is null.</exception>
        public static TempoMapManager ManageTempoMap(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().ManageTempoMap(file.TimeDivision);
        }

        /// <summary>
        ///     Gets tempo map represented by the specified events collections and time division.
        /// </summary>
        /// <param name="eventsCollections">
        ///     Collection of <see cref="EventsCollection" /> which hold events
        ///     that represent tempo map of a MIDI file.
        /// </param>
        /// <param name="timeDivision">
        ///     MIDI file time division which specifies the meaning of the time
        ///     used by events of the file.
        /// </param>
        /// <returns>
        ///     Tempo map represented by the <paramref name="eventsCollections" /> and
        ///     <paramref name="timeDivision" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventsCollections" /> is null. -or-
        ///     <paramref name="timeDivision" /> is null.
        /// </exception>
        public static TempoMap GetTempoMap(this IEnumerable<EventsCollection> eventsCollections,
            TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return eventsCollections.Any()
                ? eventsCollections.ManageTempoMap(timeDivision).TempoMap
                : new TempoMap(timeDivision);
        }

        /// <summary>
        ///     Gets tempo map represented by the specified time division and events collections of
        ///     the specified track chunks.
        /// </summary>
        /// <param name="trackChunks">
        ///     Collection of <see cref="TrackChunk" /> which hold events
        ///     that represent tempo map of a MIDI file.
        /// </param>
        /// <param name="timeDivision">
        ///     MIDI file time division which specifies the meaning of the time
        ///     used by events of the file.
        /// </param>
        /// <returns>Tempo map represented by the <paramref name="trackChunks" /> and <paramref name="timeDivision" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="trackChunks" /> is null. -or-
        ///     <paramref name="timeDivision" /> is null.
        /// </exception>
        public static TempoMap GetTempoMap(this IEnumerable<TrackChunk> trackChunks, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return trackChunks.Any()
                ? trackChunks.ManageTempoMap(timeDivision).TempoMap
                : new TempoMap(timeDivision);
        }

        /// <summary>
        ///     Gets tempo map of the specified MIDI file.
        /// </summary>
        /// <param name="file">MIDI file to get tempo map of.</param>
        /// <returns>Tempo map of the <paramref name="file" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file" /> is null.</exception>
        public static TempoMap GetTempoMap(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().Any()
                ? file.ManageTempoMap().TempoMap
                : new TempoMap(file.TimeDivision);
        }

        /// <summary>
        ///     Replaces tempo map contained in the specified collection of the <see cref="EventsCollection" /> with
        ///     another one.
        /// </summary>
        /// <param name="eventsCollections">Collection of the <see cref="EventsCollection" /> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="eventsCollections" />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventsCollections" /> is null. -or-
        ///     <paramref name="tempoMap" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="eventsCollections" /> is empty.</exception>
        public static void ReplaceTempoMap(this IEnumerable<EventsCollection> eventsCollections, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsEmptyCollection(nameof(eventsCollections),
                eventsCollections,
                $"Collection of {nameof(EventsCollection)} is empty.");

            using (var tempoMapManager = eventsCollections.ManageTempoMap(tempoMap.TimeDivision))
            {
                tempoMapManager.ReplaceTempoMap(tempoMap);
            }
        }


        /// <summary>
        ///     Replaces tempo map contained in the specified collection of the <see cref="TrackChunk" /> with
        ///     another one.
        /// </summary>
        /// <param name="trackChunks">Collection of the <see cref="TrackChunk" /> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="trackChunks" />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="trackChunks" /> is null. -or-
        ///     <paramref name="tempoMap" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="trackChunks" /> is empty.</exception>
        public static void ReplaceTempoMap(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsEmptyCollection(nameof(trackChunks),
                trackChunks,
                $"Collection of {nameof(TrackChunk)} is empty.");

            trackChunks.Select(c => c.Events).ReplaceTempoMap(tempoMap);
        }

        /// <summary>
        ///     Replaces tempo map contained in the specified <see cref="MidiFile" /> with another one.
        /// </summary>
        /// <param name="file"><see cref="MidiFile" /> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="file" />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="file" /> is null. -or-
        ///     <paramref name="tempoMap" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="file" /> is empty.</exception>
        public static void ReplaceTempoMap(this MidiFile file, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var trackChunks = file.GetTrackChunks();
            ThrowIfArgument.IsEmptyCollection(nameof(trackChunks),
                trackChunks,
                $"Collection of {nameof(TrackChunk)} of the file is empty.");

            trackChunks.ReplaceTempoMap(tempoMap);

            file.TimeDivision = tempoMap.TimeDivision.Clone();
        }

        #endregion
    }
}