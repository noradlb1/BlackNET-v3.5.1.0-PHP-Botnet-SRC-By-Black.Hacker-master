<?php
include_once APP_PATH . '/vendor/phpmailer/phpmailer/src/PHPMailer.php';
include_once APP_PATH . '/vendor/phpmailer/phpmailer/src/SMTP.php';
include_once APP_PATH . '/vendor/phpmailer/phpmailer/src/Exception.php';

use PHPMailer\PHPMailer\Exception;
use PHPMailer\PHPMailer\PHPMailer;

/*
Simple Class that handles emails created using PHPMailer and PHP mail();
 */

class Mailer
{
    private $db;

    public function __construct()
    {
        $this->db = new Database;
    }
    // Get SMTP data to use with PHPMailer
    public function getSMTP($id)
    {
        $this->db->query("SELECT * FROM smtp WHERE id = :id");

        $this->db->bind(":id", $id, PDO::PARAM_INT);

        if ($this->db->execute()) {
            return $this->db->single();

        }
    }

    // Update SMTP Data
    public function setSMTP($id, $smtphost, $smtpuser, $smtppassword, $port, $security_type, $status)
    {
        $this->db->query("UPDATE smtp SET
        smtphost = :host ,
        smtpuser = :user,
        smtppassword = :password,
        port = :port,
        security_type = :type,
        status = :status
        WHERE id = :id");

        $this->db->bind(":host", $smtphost, PDO::PARAM_STR);
        $this->db->bind(":user", $smtpuser, PDO::PARAM_STR);
        $this->db->bind(":password", base64_encode($smtppassword), PDO::PARAM_STR);
        $this->db->bind(":port", $port, PDO::PARAM_INT);
        $this->db->bind(":type", $security_type, PDO::PARAM_STR);
        $this->db->bind(":status", $status, PDO::PARAM_STR);
        $this->db->bind(":id", $id, PDO::PARAM_INT);

        if ($this->db->execute()) {
            return 'SMTP Updated';
        }
    }

    /*
    check if SMTP is enabled then use PHPMailer to send a premade messgae
    if not this function will use mail() to send messages
     */
    public function sendmail($email, $subject, $body)
    {
        $smtpdata = $this->getSMTP(1);
        $mail = new PHPMailer(true);
        try {
            if ($smtpdata->status === "on") {
                // PHPMailer settings
                $mail->isSMTP();
                $mail->Host = $smtpdata->security_type . "://" . $smtpdata->smtphost . ":" . $smtpdata->port;
                $mail->SMTPAuth = true;
                $mail->Username = $smtpdata->smtpuser;
                $mail->Password = base64_decode($smtpdata->smtppassword);

                $mail->setFrom($smtpdata->smtpuser, 'BlackNET');
                $mail->addAddress($email);

                $mail->isHTML(true);
                $mail->Subject = $subject;
                $mail->Body = $body;

                if ($mail->send()) {
                    return true;
                } else {
                    return false;
                }
            } else {
                // Headers to enable HTML in mail();
                $from = ADMIN_EMAIL;
                $headers = 'MIME-Version: 1.0' . "\r\n";
                $headers .= 'Content-type: text/html; charset=iso-8859-1' . "\r\n";

                // Create email headers
                $headers .= 'From: ' . $from . "\r\n" .
                'Reply-To: ' . $from . "\r\n" .
                'X-Mailer: PHP/' . phpversion();
                if (mail($email, $subject, $body, $headers)) {
                    return true;
                } else {
                    return false;
                }
            }
        } catch (Exception $e) {
            return "Message could not be sent. Mailer Error: {$mail->ErrorInfo}";
        }
    }
}
