import { useState } from 'react';
import { ErrorMessage } from '@hookform/error-message';
import clsx from 'clsx';
import PropTypes from 'prop-types';
import ErrorMessageTag from './ErrorMessage';

export const LimitedTextarea = ({
  name,
  rows,
  placeholder,
  value,
  limit,
  onChange,
  inputRef,
  register,
  errors,
  classNames,
}) => {
  const [length, setLength] = useState(value?.length || 0);
  let change = onChange ? onChange : (event) => setLength(event.target.value.length);

  const classes = clsx(
    'block w-full px-3 py-2 text-sm leading-tight text-gray-800 placeholder-gray-400 transition duration-100 ease-in-out bg-white  focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:outline-none focus:ring-opacity-50',
    classNames
  );
  return (
    <div className="flex flex-col flex-grow">
      <textarea
        name={name}
        id={name}
        rows={rows}
        type="textarea"
        defaultValue={value}
        ref={inputRef}
        maxLength={limit}
        placeholder={placeholder}
        className={classes}
        onChange={change}
        {...register(name)}
      ></textarea>
      {length > 0 && <span className="self-end text-xs text-gray-400">{limit - length} characters left</span>}
      <ErrorMessage errors={errors} name={name} as={ErrorMessageTag} />
    </div>
  );
};

LimitedTextarea.propTypes = {
  /**
   * The property name used by react hook form
   */
  name: PropTypes.string,
  /**
   * The help text to display
   */
  placeholder: PropTypes.string,
  /**
   * The value to preset the input to
   */
  value: PropTypes.string,
  /**
   * The number of rows to have in the textarea
   */
  rows: PropTypes.string,
  /**
   * The character count limit
   */
  limit: PropTypes.number.isRequired,
  /**
   * The ref property for use with registering with react hook form
   */
  inputRef: PropTypes.oneOfType([PropTypes.object, PropTypes.func]),
  /**
   * The function to execute when the text area value changes
   */
  onChange: PropTypes.func,
};

LimitedTextarea.defaultProps = {
  name: null,
  placeholder: null,
  value: '',
  rows: 3,
  limit: 500,
  inputRef: null,
  onChange: undefined,
};
