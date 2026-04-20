using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant telemetry collector.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetTelemetryResponse : QdrantResponseBase<JsonObject>
{
    /// <summary>
    /// Represents telemetry data.
    /// </summary>
    public class TelemetryResponseUnit
    {

    }

    /*
    {
  "usage": {
    "hardware": {
      "cpu": 1,
      "payload_io_read": 1,
      "payload_io_write": 1,
      "payload_index_io_read": 1,
      "payload_index_io_write": 1,
      "vector_io_read": 1,
      "vector_io_write": 1
    },
    "inference": {
      "models": {}
    }
  },
  "time": 0.002,
  "status": "ok",
  "result": {
    "id": "string",
    "app": {
      "name": "string",
      "version": "string",
      "startup": "2024-01-15T09:30:00Z",
      "features": {
        "debug": true,
        "service_debug_feature": true,
        "recovery_mode": true,
        "gpu": true,
        "rocksdb": true
      },
      "runtime_features": {
        "all": false,
        "payload_index_skip_rocksdb": true,
        "payload_index_skip_mutable_rocksdb": true,
        "payload_storage_skip_rocksdb": true,
        "incremental_hnsw_building": true,
        "migrate_rocksdb_id_tracker": true,
        "migrate_rocksdb_vector_storage": false,
        "migrate_rocksdb_payload_storage": false,
        "migrate_rocksdb_payload_indices": false,
        "appendable_quantization": true
      },
      "hnsw_global_config": {
        "healing_threshold": 0.3
      },
      "system": {
        "distribution": "string",
        "distribution_version": "string",
        "is_docker": true,
        "cores": 1,
        "ram_size": 1,
        "disk_size": 1,
        "cpu_flags": "string",
        "cpu_endian": "little",
        "gpu_devices": [
          {
            "name": "string"
          }
        ]
      },
      "jwt_rbac": true,
      "hide_jwt_dashboard": true
    },
    "collections": {
      "number_of_collections": 1,
      "max_collections": 1,
      "collections": [
        {
          "id": "string",
          "init_time_ms": 1,
          "config": {
            "params": {
              "vectors": {},
              "shard_number": 1,
              "sharding_method": "auto",
              "replication_factor": 1,
              "write_consistency_factor": 1,
              "read_fan_out_factor": 1,
              "on_disk_payload": true,
              "sparse_vectors": {}
            },
            "hnsw_config": {
              "m": 1,
              "ef_construct": 1,
              "full_scan_threshold": 1,
              "max_indexing_threads": 0,
              "on_disk": true,
              "payload_m": 1,
              "inline_storage": true
            },
            "optimizer_config": {
              "deleted_threshold": 1.1,
              "vacuum_min_vector_number": 1,
              "default_segment_number": 1,
              "max_segment_size": 1,
              "memmap_threshold": 1,
              "indexing_threshold": 1,
              "flush_interval_sec": 1,
              "max_optimization_threads": 1
            },
            "wal_config": {
              "wal_capacity_mb": 1,
              "wal_segments_ahead": 1,
              "wal_retain_closed": 1
            },
            "quantization_config": {
              "scalar": {
                "type": "int8",
                "quantile": 1.1,
                "always_ram": true
              }
            },
            "strict_mode_config": {
              "enabled": true,
              "max_query_limit": 1,
              "max_timeout": 1,
              "unindexed_filtering_retrieve": true,
              "unindexed_filtering_update": true,
              "search_max_hnsw_ef": 1,
              "search_allow_exact": true,
              "search_max_oversampling": 1.1,
              "upsert_max_batchsize": 1,
              "max_collection_vector_size_bytes": 1,
              "read_rate_limit": 1,
              "write_rate_limit": 1,
              "max_collection_payload_size_bytes": 1,
              "max_points_count": 1,
              "filter_max_conditions": 1,
              "condition_max_size": 1,
              "multivector_config": {},
              "sparse_config": {},
              "max_payload_index_count": 1
            },
            "uuid": "string",
            "metadata": {}
          },
          "shards": [
            {
              "id": 1,
              "key": "region_1",
              "local": {
                "variant_name": "string",
                "status": "green",
                "total_optimized_points": 1,
                "vectors_size_bytes": 1,
                "payloads_size_bytes": 1,
                "num_points": 1,
                "num_vectors": 1,
                "num_vectors_by_name": {},
                "segments": [
                  {
                    "info": {
                      "segment_type": "plain",
                      "num_vectors": 1,
                      "num_points": 1,
                      "num_indexed_vectors": 1,
                      "num_deleted_vectors": 1,
                      "vectors_size_bytes": 1,
                      "payloads_size_bytes": 1,
                      "ram_usage_bytes": 1,
                      "disk_usage_bytes": 1,
                      "is_appendable": true,
                      "index_schema": {},
                      "vector_data": {}
                    },
                    "config": {
                      "vector_data": {},
                      "sparse_vector_data": {},
                      "payload_storage_type": {
                        "type": "in_memory"
                      }
                    },
                    "vector_index_searches": [
                      {
                        "index_name": "string",
                        "unfiltered_plain": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "unfiltered_hnsw": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "unfiltered_sparse": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "filtered_plain": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "filtered_small_cardinality": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "filtered_large_cardinality": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "filtered_exact": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "filtered_sparse": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        },
                        "unfiltered_exact": {
                          "count": 1,
                          "fail_count": 1,
                          "avg_duration_micros": 1.1,
                          "min_duration_micros": 1.1,
                          "max_duration_micros": 1.1,
                          "total_duration_micros": 1,
                          "last_responded": "2024-01-15T09:30:00Z"
                        }
                      }
                    ],
                    "payload_field_indices": [
                      {
                        "field_name": "string",
                        "index_type": "string",
                        "points_values_count": 1,
                        "points_count": 1,
                        "histogram_bucket_size": 1
                      }
                    ]
                  }
                ],
                "optimizations": {
                  "status": "ok",
                  "optimizations": {
                    "count": 1,
                    "fail_count": 1,
                    "avg_duration_micros": 1.1,
                    "min_duration_micros": 1.1,
                    "max_duration_micros": 1.1,
                    "total_duration_micros": 1,
                    "last_responded": "2024-01-15T09:30:00Z"
                  },
                  "log": [
                    {
                      "name": "string",
                      "segment_ids": [
                        1
                      ],
                      "status": "optimizing",
                      "start_at": "2024-01-15T09:30:00Z",
                      "end_at": "2024-01-15T09:30:00Z"
                    }
                  ]
                },
                "async_scorer": true,
                "indexed_only_excluded_vectors": {}
              },
              "remote": [
                {
                  "shard_id": 1,
                  "peer_id": 1,
                  "searches": {
                    "count": 1,
                    "fail_count": 1,
                    "avg_duration_micros": 1.1,
                    "min_duration_micros": 1.1,
                    "max_duration_micros": 1.1,
                    "total_duration_micros": 1,
                    "last_responded": "2024-01-15T09:30:00Z"
                  },
                  "updates": {
                    "count": 1,
                    "fail_count": 1,
                    "avg_duration_micros": 1.1,
                    "min_duration_micros": 1.1,
                    "max_duration_micros": 1.1,
                    "total_duration_micros": 1,
                    "last_responded": "2024-01-15T09:30:00Z"
                  }
                }
              ],
              "replicate_states": {},
              "partial_snapshot": {
                "ongoing_create_snapshot_requests": 1,
                "is_recovering": true,
                "recovery_timestamp": 1
              }
            }
          ],
          "transfers": [
            {
              "shard_id": 1,
              "to_shard_id": 1,
              "from": 1,
              "to": 1,
              "sync": true,
              "method": "stream_records",
              "comment": "string"
            }
          ],
          "resharding": [
            {
              "direction": "up",
              "shard_id": 1,
              "peer_id": 1,
              "shard_key": "region_1"
            }
          ],
          "shard_clean_tasks": {}
        }
      ],
      "snapshots": [
        {
          "id": "string",
          "running_snapshots": 1,
          "running_snapshot_recovery": 1,
          "total_snapshot_creations": 1
        }
      ]
    },
    "cluster": {
      "enabled": true,
      "status": {
        "number_of_peers": 1,
        "term": 1,
        "commit": 1,
        "pending_operations": 1,
        "role": "Follower",
        "is_voter": true,
        "peer_id": 1,
        "consensus_thread_status": {
          "consensus_thread_status": "working",
          "last_update": "2024-01-15T09:30:00Z"
        }
      },
      "config": {
        "grpc_timeout_ms": 1,
        "p2p": {
          "connection_pool_size": 1
        },
        "consensus": {
          "max_message_queue_size": 1,
          "tick_period_ms": 1,
          "bootstrap_timeout_sec": 1
        }
      },
      "peers": {},
      "peer_metadata": {},
      "metadata": {}
    },
    "requests": {
      "rest": {
        "responses": {}
      },
      "grpc": {
        "responses": {}
      }
    },
    "memory": {
      "active_bytes": 1,
      "allocated_bytes": 1,
      "metadata_bytes": 1,
      "resident_bytes": 1,
      "retained_bytes": 1
    },
    "hardware": {
      "collection_data": {}
    }
  }
}


    */

}
