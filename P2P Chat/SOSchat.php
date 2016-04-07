<?php 
$data ;
$servername = "localhost";
$username = "username";
$password = "password";
$dbname = "myDB";

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
} 


$sql = "SELECT * FROM hosts";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
    // output data of each row
    while($row = $result->fetch_assoc()) {
        data .= "<host><ip>" . $row["ip"]. "</ip><port>" . $row["port"]. "</port><ID>" . $row["id"] . "</ID><type>" . $row["type"] . "</type></host>";
    }
} 

$sql = "SELECT * FROM users";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
    // output data of each row
    while($row = $result->fetch_assoc()) {
        data .= "<user><ip>" . $row["ip"]. "</ip><port>" . $row["port"]. "</port><ID>" . $row["id"] . "</ID></user>";
    }
}


 
$conn->close();

echo "<?xml version='1.0' encoding='utf-8' ?>
  
    <host>
      <ip>http://punksky.xyz/SOSChat.php</ip>
      <type>http</type>
      <port>80</port>
      <ID>PunkSky.Root</ID>
      <custom></custom>
    </host>" . data;
  ?>