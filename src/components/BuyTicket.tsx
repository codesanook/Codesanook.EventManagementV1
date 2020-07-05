import React, { useState, useEffect, useRef } from 'react';

// context of Ticket
interface IProps {
    ticketPrice: string;
    availableTicketCount: number;
    actionUrl: string
}

const BuyTicket = (prop: IProps) => {
    const [numberOfAttendees, setNumberOfAttendees] = useState(0);

    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => { 
        console.log(`on change with new value ${e.target.value}`);
        setNumberOfAttendees(e.target.value);
    };

    const ticketOptions = [...Array(prop.availableTicketCount).keys()].map(n => n + 1);
    const selectElement = useRef(null);

    useEffect(() => {
        // Set value here to prevent problem with browser back keep old selected value
        setNumberOfAttendees(selectElement.current.value);
    }, []); //That will ensure the useEffect only runs once.

    return (
        <>
            <a className="btn btn-primary" 
                href={`${prop.actionUrl}?numberOfAttendees=${numberOfAttendees}`} 
                role="button">
                Buy ticket
            </a>

            <label className="lbl-price mx-5">&#3647;150.00</label>
            <span>x</span>
            <select className="form-control ddl-amount" 
                onChange={handleChange} 
                defaultValue={ticketOptions[0]}
                ref={selectElement}>
                {ticketOptions.map(number =>
                    <option key={number.toString()} value={number}>
                        {number}
                    </option>
                )}
            </select>
        </>
    )
};

export default BuyTicket;
