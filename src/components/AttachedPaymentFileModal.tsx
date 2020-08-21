import { Button, Modal } from 'react-bootstrap';
import React, { useState } from 'react';

interface IProps {
    attachedPaymentFileUrl: string;
}

const AttachedPaymentFileModal = ({ attachedPaymentFileUrl }): IProps => {

    const [show, setShow] = useState(false);

    const handleClose = () => setShow(false);
    const handleShow = () => setShow(true);

    return (
        <>
            <Button variant="primary" onClick={handleShow}>
                Show attached payment file
            </Button>
            <Modal show={show} onHide={handleClose} centered>
                <Modal.Header closeButton>
                    <Modal.Title className="mx-auto pl-5">
                        Payment file
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <img src={attachedPaymentFileUrl} />
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

export default AttachedPaymentFileModal;
