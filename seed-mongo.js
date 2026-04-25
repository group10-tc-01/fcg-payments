// Seed: payment_reports collection
// Espelha os payments Approved/Rejected inseridos no SQL Server
// (Pending não vai para o MongoDB — só é inserido após Approve/Reject)

db = db.getSiblingDB('Payments');

db.payment_reports.insertMany([
  // User 1 - Approved
  {
    _id: "d1000000-0000-0000-0000-000000000001",
    UserId:      "b1000000-0000-0000-0000-000000000001",
    GameId:      "c1000000-0000-0000-0000-000000000001",
    Amount:      NumberDecimal("49.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 5*3600*1000)
  },
  {
    _id: "d1000000-0000-0000-0000-000000000002",
    UserId:      "b1000000-0000-0000-0000-000000000001",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("99.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 4*3600*1000)
  },
  {
    _id: "d1000000-0000-0000-0000-000000000003",
    UserId:      "b1000000-0000-0000-0000-000000000001",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("199.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 3*3600*1000)
  },
  // User 1 - Rejected
  {
    _id: "d1000000-0000-0000-0000-000000000004",
    UserId:      "b1000000-0000-0000-0000-000000000001",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("500.00"),
    Status:      "Rejected",
    ProcessedAt: new Date(new Date().getTime() - 2*3600*1000)
  },

  // User 2 - Approved
  {
    _id: "d1000000-0000-0000-0000-000000000005",
    UserId:      "b1000000-0000-0000-0000-000000000002",
    GameId:      "c1000000-0000-0000-0000-000000000001",
    Amount:      NumberDecimal("99.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 6*3600*1000)
  },
  // User 2 - Rejected
  {
    _id: "d1000000-0000-0000-0000-000000000006",
    UserId:      "b1000000-0000-0000-0000-000000000002",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("300.00"),
    Status:      "Rejected",
    ProcessedAt: new Date(new Date().getTime() - 5*3600*1000)
  },
  {
    _id: "d1000000-0000-0000-0000-000000000007",
    UserId:      "b1000000-0000-0000-0000-000000000002",
    GameId:      "c1000000-0000-0000-0000-000000000001",
    Amount:      NumberDecimal("250.00"),
    Status:      "Rejected",
    ProcessedAt: new Date(new Date().getTime() - 1*3600*1000)
  },

  // User 3 - Approved
  {
    _id: "d1000000-0000-0000-0000-000000000008",
    UserId:      "b1000000-0000-0000-0000-000000000003",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("29.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 8*3600*1000)
  },
  {
    _id: "d1000000-0000-0000-0000-000000000009",
    UserId:      "b1000000-0000-0000-0000-000000000003",
    GameId:      "c1000000-0000-0000-0000-000000000001",
    Amount:      NumberDecimal("59.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 7*3600*1000)
  },

  // User 5 - Approved
  {
    _id: "d1000000-0000-0000-0000-000000000011",
    UserId:      "b1000000-0000-0000-0000-000000000005",
    GameId:      "c1000000-0000-0000-0000-000000000001",
    Amount:      NumberDecimal("149.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 10*3600*1000)
  },
  {
    _id: "d1000000-0000-0000-0000-000000000012",
    UserId:      "b1000000-0000-0000-0000-000000000005",
    GameId:      "c1000000-0000-0000-0000-000000000002",
    Amount:      NumberDecimal("199.99"),
    Status:      "Approved",
    ProcessedAt: new Date(new Date().getTime() - 9*3600*1000)
  }
]);

print("payment_reports inseridos: " + db.payment_reports.countDocuments());
