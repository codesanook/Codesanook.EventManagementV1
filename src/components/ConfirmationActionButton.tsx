import { Button, Modal } from 'react-bootstrap';
import React, { useState } from 'react';

interface IProps {
    actionUrl: string;
    formParameters: any;
    actionButtonMessage: string;
    confirmationTitle: string;
    confirmationMessage: string;
}

// Credit https://stackoverflow.com/a/133997/1872200
// https://www.typescriptlang.org/docs/handbook/functions.html#optional-and-default-parameters
/**
 * sends a request to the specified url from a form. this will change the window location.
 * @param {string} path the path to send the post request to
 * @param {object} params the paramiters to add to the url
 * @param {string} [method=post] the method to use on the form
 */
function post(path, params = {}, method = 'post') {

  // The rest of this code assumes you are not using a library.
  // It can be made less wordy if you use one.
  const form = document.createElement('form');
  form.method = method;
  form.action = path;

  for (const key in params) {
    if (params.hasOwnProperty(key)) {
      const hiddenField = document.createElement('input');
      hiddenField.type = 'hidden';
      hiddenField.name = key;
      hiddenField.value = params[key];
      form.appendChild(hiddenField);
    }
  }

  document.body.appendChild(form);
  form.submit();
}

const ConfirmationActionButton = ({ 
    actionUrl, 
    formParameters, 
    actionButtonMessage, 
    confirmationTitle, 
    confirmationMessage,
}): IProps => {

    const [show, setShow] = useState(false);
    const handleClose = () => setShow(false);
    const handleShow = () => setShow(true);
    const handleConfirmedAction = () => {
        post(actionUrl, formParameters);  
    };

    return (
        <>
            <Button variant="primary" onClick={handleShow}>
                {actionButtonMessage}
            </Button>

            <Modal show={show} onHide={handleClose} centered>
                <Modal.Header closeButton>
                    <Modal.Title className="mx-auto pl-5"> {confirmationTitle} </Modal.Title>
                </Modal.Header>
                <Modal.Body className="text-center">
                    {confirmationMessage}
                </Modal.Body>
                <Modal.Footer className="btn-action">
                    <Button variant="primary" onClick={handleConfirmedAction}>
                        OK
                    </Button>
                    <Button variant="secondary" onClick={handleClose}>
                        Cancel
                    </Button>
                </Modal.Footer>
            </Modal>
        </>
    );
};

export default ConfirmationActionButton;
