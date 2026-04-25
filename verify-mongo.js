db = db.getSiblingDB('Payments');
print("Total docs: " + db.payment_reports.countDocuments());
print("Approved: " + db.payment_reports.countDocuments({Status:"Approved"}));
print("Rejected: " + db.payment_reports.countDocuments({Status:"Rejected"}));
