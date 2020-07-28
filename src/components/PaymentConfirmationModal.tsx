// or less ideally
import { Button, Modal } from 'react-bootstrap';
import React, { useState } from 'react';


interface IProps {
    paymentConfirmationFileUrl: string;
}

const PaymentConfirmationModal = ({ paymentConfirmationFileUrl }): IProps => {

    const [show, setShow] = useState(false);

    const handleClose = () => setShow(false);
    const handleShow = () => setShow(true);

    return (
        <>
            <Button variant="primary" onClick={handleShow}>
                Show payment confirmation file
            </Button>
            <Modal show={show} onHide={handleClose} centered>
                <Modal.Header closeButton>
                    <Modal.Title className="mx-auto pl-5">
                        Payment confirmation file
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <img src={paymentConfirmationFileUrl} />
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleClose}>
                        Close
                    </Button>
                </Modal.Footer>
            </Modal>
        </>

    );

};

export default PaymentConfirmationModal;
