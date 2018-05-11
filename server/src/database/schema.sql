CREATE TABLE payment (
  bc_tx_id STRING(128) NOT NULL,
  user_id STRING(64) NOT NULL,
  iap_tx_id STRING(256) NOT NULL,
  last_update TIMESTAMP NOT NULL,
  INDEX(user_id),
  INDEX(last_update)
) PRIMARY KEY(user_id DESC);
​
